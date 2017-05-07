﻿// <copyright file="ITokenRenderer.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;
using Stubble.Core.Tokens;

namespace Stubble.Core.Renderers.Interfaces
{
    /// <summary>
    /// An interface representing a TokenRenderer
    /// </summary>
    public interface ITokenRenderer
    {
        /// <summary>
        /// Does the renderer accept the current tag
        /// </summary>
        /// <param name="renderer">The renderer</param>
        /// <param name="obj">The current tag</param>
        /// <returns>If the renderer accepts the tag</returns>
        bool Accept(RendererBase renderer, MustacheToken obj);

        /// <summary>
        /// Writes the tag using the renderer
        /// </summary>
        /// <param name="renderer">The renderer to write with</param>
        /// <param name="objectToRender">The tag to write</param>
        /// <param name="context">The context to write the token</param>
        void Write(RendererBase renderer, MustacheToken objectToRender, Context context);

        /// <summary>
        /// Writes the tag using the renderer
        /// </summary>
        /// <param name="renderer">The renderer to write with</param>
        /// <param name="objectToRender">The tag to write</param>
        /// <param name="context">The context to write the token</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task WriteAsync(RendererBase renderer, MustacheToken objectToRender, Context context);
    }
}
