﻿// <copyright file="SpecTests.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Stubble.Core.Tests.Spec
{

    [Collection("SpecCommentTests")]
    public class CommentsTests : SpecTestBase
    {
        public CommentsTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> Spec_Comments(bool skip)
        {
            return SpecTestHelper.GetTests("comments", skip).Select(test => new object[] { test });
        }

        [Theory]
        [MemberData(nameof(Spec_Comments), true)]
        public new void It_Can_Pass_Spec_Tests(SpecTest data)
        {
            base.It_Can_Pass_Spec_Tests(data);
        }
    }

    [Collection("SpecDelimiterTests")]
    public class DelimiterTests : SpecTestBase
    {
        public DelimiterTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> Spec_Delimiters(bool skip)
        {
            return SpecTestHelper.GetTests("delimiters", skip).Select(test => new object[] { test });
        }

        [Theory]
        [MemberData(nameof(Spec_Delimiters), true)]
        public new void It_Can_Pass_Spec_Tests(SpecTest data)
        {
            base.It_Can_Pass_Spec_Tests(data);
        }
    }

    [Collection("SpecInterpolationTests")]
    public class InterpolationTests : SpecTestBase
    {
        public InterpolationTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> Spec_Interpolation(bool skip)
        {
            return SpecTestHelper.GetTests("interpolation", skip).Select(test => new object[] { test });
        }

        [Theory]
        [MemberData(nameof(Spec_Interpolation), true)]
        public new void It_Can_Pass_Spec_Tests(SpecTest data)
        {
            base.It_Can_Pass_Spec_Tests(data);
        }
    }

    [Collection("InvertedTestsCollection")]
    public class InvertedTests : SpecTestBase
    {
        public InvertedTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> Spec_Inverted(bool skip)
        {
            return SpecTestHelper.GetTests("inverted", skip).Select(test => new object[] { test });
        }

        [Theory]
        [MemberData(nameof(Spec_Inverted), true)]
        public new void It_Can_Pass_Spec_Tests(SpecTest data)
        {
            base.It_Can_Pass_Spec_Tests(data);
        }
    }

    [Collection("PartialsTestsCollection")]
    public class PartialsTests : SpecTestBase
    {
        public PartialsTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> Spec_Partials(bool skip)
        {
            return SpecTestHelper.GetTests("partials", skip).Select(test => new object[] { test });
        }

        [Theory]
        [MemberData(nameof(Spec_Partials), true)]
        public new void It_Can_Pass_Spec_Tests(SpecTest data)
        {
            base.It_Can_Pass_Spec_Tests(data);
        }
    }

    [Collection("SectionsTestsCollection")]
    public class SectionsTests : SpecTestBase
    {
        public SectionsTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> Spec_Sections(bool skip)
        {
            return SpecTestHelper.GetTests("sections", skip).Select(test => new object[] { test });
        }

        [Theory]
        [MemberData(nameof(Spec_Sections), true)]
        public new void It_Can_Pass_Spec_Tests(SpecTest data)
        {
            base.It_Can_Pass_Spec_Tests(data);
        }
    }

    [Collection("LambdaTestsCollection")]
    public class LambdaTests : SpecTestBase
    {
        public static AsyncLocal<int> GlobalInt = new AsyncLocal<int>();

        public LambdaTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> Spec_Lambdas()
        {
            return new[]
            {
                new SpecTest()
                {
                    Name = "Interpolation",
                    Desc = "A lambda's return value should be interpolated.",
                    Data = new Dictionary<string, object>
                    {
                        { "lambda", new Func<object>(() => "world") }
                    },
                    Template = "Hello, {{lambda}}!",
                    Expected = "Hello, world!"
                },
                new SpecTest()
                {
                    Name = "Interpolation - Expansion",
                    Desc = "A lambda's return value should be parsed.",
                    Data = new Dictionary<string, object>
                    {
                        { "planet", "world" },
                        { "lambda", new Func<object>(() => "{{planet}}") }
                    },
                    Template = "Hello, {{lambda}}!",
                    Expected = "Hello, world!"
                },
                new SpecTest()
                {
                    Name = "Interpolation - Alternate Delimiters",
                    Desc = "A lambda's return value should parse with the default delimiters.",
                    Data = new Dictionary<string, object>
                    {
                        { "planet", "world" },
                        { "lambda", new Func<object>(() => "|planet| => {{planet}}") }
                    },
                    Template = "{{= | | =}}\nHello, (|&lambda|)!",
                    Expected = "Hello, (|planet| => world)!"
                },
                new SpecTest()
                {
                    Name = "Interpolation - Multiple Calls",
                    Desc = "Interpolated lambdas should not be cached.",
                    Data = new Dictionary<string, object>
                    {
                        { "lambda", new Func<object>(() => ++LambdaTests.GlobalInt.Value) }
                    },
                    Template = "{{lambda}} == {{lambda}} == {{lambda}}",
                    Expected = "1 == 2 == 3"
                },
                new SpecTest()
                {
                    Name = "Escaping",
                    Desc = "Lambda results should be appropriately escaped.",
                    Data = new Dictionary<string, object>
                    {
                        { "lambda", new Func<object>(() => ">") }
                    },
                    Template = "<{{lambda}}{{{lambda}}}",
                    Expected = "<&gt;>"
                },
                new SpecTest()
                {
                    Name = "Section",
                    Desc = "Lambdas used for sections should receive the raw section string.",
                    Data = new Dictionary<string, object>
                    {
                        { "x", "error" },
                        { "lambda", new Func<string, object>(txt => txt == "{{x}}" ? "yes" : "no") }
                    },
                    Template = "<{{#lambda}}{{x}}{{/lambda}}>",
                    Expected = "<yes>"
                },
                new SpecTest()
                {
                    Name = "Section - Expansion",
                    Desc = "Lambdas used for sections should have their results parsed.",
                    Data = new Dictionary<string, object>
                    {
                        { "planet", "Earth" },
                        { "lambda", new Func<string, object>(txt => txt + "{{planet}}" + txt) }
                    },
                    Template = "<{{#lambda}}-{{/lambda}}>",
                    Expected = "<-Earth->"
                },
                new SpecTest()
                {
                    Name = "Section - Alternate Delimiters",
                    Desc = "Lambdas used for sections should parse with the current delimiters.",
                    Data = new Dictionary<string, object>
                    {
                        { "planet", "Earth" },
                        { "lambda", new Func<string, object>(txt => txt + "{{planet}} => |planet|" + txt) }
                    },
                    Template = "{{= | | =}}<|#lambda|-|/lambda|>",
                    Expected = "<-{{planet}} => Earth->"
                },
                new SpecTest()
                {
                    Name = "Section - Multiple Calls",
                    Desc = "Lambdas used for sections should not be cached.",
                    Data = new Dictionary<string, object>
                    {
                        { "lambda", new Func<string, object>(txt => "__" + txt + "__") }
                    },
                    Template = "{{#lambda}}FILE{{/lambda}} != {{#lambda}}LINE{{/lambda}}",
                    Expected = "__FILE__ != __LINE__"
                },
                new SpecTest()
                {
                    Name = "Inverted Section",
                    Desc = "Lambdas used for inverted sections should be considered truthy.",
                    Data = new Dictionary<string, object>
                    {
                        { "static", "static" },
                        { "lambda", new Func<string, object>(txt => false) }
                    },
                    Template = "<{{^lambda}}{{static}}{{/lambda}}>",
                    Expected = "<>"
                }
            }.Select(x => new[] { x });
        }

        [Theory]
        [MemberData(nameof(Spec_Lambdas))]
        public new void It_Can_Pass_Spec_Tests(SpecTest data)
        {
            base.It_Can_Pass_Spec_Tests(data);
        }
    }
}
