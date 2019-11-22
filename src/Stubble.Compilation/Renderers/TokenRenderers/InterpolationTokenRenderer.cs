﻿// <copyright file="InterpolationTokenRenderer.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Stubble.Compilation.Contexts;
using Stubble.Compilation.Helpers;
using Stubble.Core.Helpers;
using Stubble.Core.Tokens;

namespace Stubble.Compilation.Renderers.TokenRenderers
{
    /// <summary>
    /// A <see cref="ExpressionObjectRenderer{InterpolationTag}"/> for rendering <see cref="InterpolationToken"/>s
    /// </summary>
    public class InterpolationTokenRenderer : ExpressionObjectRenderer<InterpolationToken>
    {
        private static Type[] formatProviderTypeArgs = new[] { typeof(IFormatProvider) };

        /// <inheritdoc/>
        protected override void Write(CompilationRenderer renderer, InterpolationToken obj, CompilerContext context)
        {
            var member = obj.Content.ToString();

            var expression = context.Lookup(member);

            if (!context.CompilationSettings.SkipHtmlEncoding && obj.EscapeResult && expression != null)
            {
                Expression stringExpression;
                if (expression.Type == typeof(string))
                {
                    stringExpression = expression;
                }
                else
                {
                    var formattedToString = expression.Type
                        .GetMethod(nameof(object.ToString), formatProviderTypeArgs);

                    var item = expression.Type.GetIsValueType()
                        ? expression
                        : Expression.Coalesce(expression, Expression.Constant(string.Empty));

                    stringExpression = formattedToString is object
                        ? Expression.Call(item, formattedToString, Expression.Constant(context.CompilationSettings.CultureInfo))
                        : Expression.Call(item, expression.Type.GetMethod(nameof(object.ToString), Type.EmptyTypes));
                }

                expression = Expression.Invoke(context.CompilerSettings.EncodingFuction, stringExpression);
            }

            if (obj.Indent > 0)
            {
                var append = Expression.Call(renderer.Builder, MethodInfos.Instance.StringBuilderAppendString, Expression.Constant(new string(' ', obj.Indent)));
                renderer.AddExpressionToScope(append);
            }

            if (expression != null)
            {
                var appendMethod = typeof(StringBuilder).GetMethod("Append", new[] { expression.Type });
                var append = Expression.Call(renderer.Builder, appendMethod, expression);
                renderer.AddExpressionToScope(append);
            }
        }

        /// <inheritdoc/>
        protected override Task WriteAsync(CompilationRenderer renderer, InterpolationToken obj, CompilerContext context)
        {
            Write(renderer, obj, context);
            return TaskHelpers.CompletedTask;
        }
    }
}
