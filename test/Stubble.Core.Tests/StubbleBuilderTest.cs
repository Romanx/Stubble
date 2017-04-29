﻿// <copyright file="StubbleBuilderTest.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Collections.Specialized;
using Stubble.Core.Classes.Loaders;
using Stubble.Core.Interfaces;
using Xunit;

namespace Stubble.Core.Tests
{
    public static class CustomBuilderExtensions
    {
        public static CustomBuilder SetCustomBuilder<T>(this IStubbleBuilder<T> builder)
        {
            return builder.SetBuilderType<CustomBuilder>();
        }
    }

    public class StubbleBuilderTest
    {
        [Fact]
        public void It_Can_Add_Add_Value_Getters()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
                                .AddValueGetter(typeof(string), (o, s) => null);

            Assert.Contains(typeof(string), builder.ValueGetters.Keys);
            Assert.Null(builder.ValueGetters[typeof(string)](null, null));
        }

        [Fact]
        public void It_Can_Add_Token_Getters()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
                            .AddTokenGetter("MyToken", (s, tags) => null);

            Assert.Contains("MyToken", builder.TokenGetters.Keys);
            Assert.Null(builder.TokenGetters["MyToken"](null, null));
        }

        [Fact]
        public void It_Can_Add_Enumeration_Converters()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
                            .AddEnumerationConversion(typeof(NameValueCollection), (obj) => null);

            Assert.Contains(typeof(NameValueCollection), builder.EnumerationConverters.Keys);
            Assert.Null(builder.EnumerationConverters[typeof(NameValueCollection)](null));
        }

        [Fact]
        public void It_Can_Add_Truthy_Checks()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
                .AddTruthyCheck((val) =>
                {
                    if (val is string)
                    {
                        return val.Equals("Foo");
                    }
                    return null;
                });

            Assert.Equal(1, builder.TruthyChecks.Count);
            Assert.True(builder.TruthyChecks[0]("Foo"));
            Assert.False(builder.TruthyChecks[0]("Bar"));
            Assert.Null(builder.TruthyChecks[0](null));
        }

        [Fact]
        public void It_Can_Set_Template_Loader()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
                .SetTemplateLoader(new DictionaryLoader(new Dictionary<string, string> { { "test", "{{foo}}" } }));

            Assert.NotNull(builder.TemplateLoader);
            Assert.True(builder.TemplateLoader is DictionaryLoader);
        }

        [Fact]
        public void It_Can_Set_A_Partial_Template_Loader()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
                   .SetPartialTemplateLoader(new DictionaryLoader(new Dictionary<string, string> { { "test", "{{foo}}" } }));

            Assert.NotNull(builder.PartialTemplateLoader);
            Assert.True(builder.PartialTemplateLoader is DictionaryLoader);
        }

        [Fact]
        public void It_Adds_To_Composite_Loader_If_One_Is_Defined()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
                .SetTemplateLoader(new CompositeLoader(new DictionaryLoader(new Dictionary<string, string> { { "test", "{{foo}}" } })));

            builder.AddToTemplateLoader(new DictionaryLoader(new Dictionary<string, string> { { "test2", "{{bar}}" } }));

            Assert.NotNull(builder.TemplateLoader);
            Assert.True(builder.TemplateLoader is CompositeLoader);
        }

        [Fact]
        public void It_Should_Be_Able_To_Set_Ignore_Case_On_Key_Lookup()
        {
            var builder = (StubbleBuilder)new StubbleBuilder()
               .SetIgnoreCaseOnKeyLookup(true);

            Assert.True(builder.IgnoreCaseOnKeyLookup);
        }

        [Fact]
        public void It_Can_Build_Stubble_Instance()
        {
            var stubble = new StubbleBuilder().Build();

            Assert.NotNull(stubble);
            Assert.NotNull(stubble.Registry.ValueGetters);
            Assert.NotNull(stubble.Registry.TokenGetters);
            Assert.NotNull(stubble.Registry.TokenMatchRegex);
            Assert.NotNull(stubble.Registry.TruthyChecks);
            Assert.True(stubble.Registry.TemplateLoader is StringLoader);
            Assert.False(stubble.Registry.IgnoreCaseOnKeyLookup);
            Assert.Null(stubble.Registry.PartialTemplateLoader);

            Assert.NotEmpty(stubble.Registry.ValueGetters);
            Assert.NotEmpty(stubble.Registry.TokenGetters);
        }

        [Fact]
        public void DifferentBuildersBuildDifferentResults()
        {
            var stubble = new CustomBuilder().Build();
            Assert.IsType<string>(stubble);
        }

        [Fact]
        public void It_Can_ConvertOneBuilderToAnother()
        {
            var stubble = new StubbleBuilder()
                            .SetBuilderType<CustomBuilder>()
                            .Build();
            Assert.IsType<string>(stubble);
        }

        [Fact]
        public void It_Can_Override_InAn_ExtensionMethod()
        {
            var stubble = new StubbleBuilder()
                            .SetCustomBuilder()
                            .Build();
            Assert.IsType<string>(stubble);
        }
    }

    public class CustomBuilder : StubbleBuilder<string>
    {
        public override string Build()
        {
            return "";
        }
    }
}
