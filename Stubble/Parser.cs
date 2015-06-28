﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stubble.Core.Classes;
using Stubble.Core.Classes.Tokens;

namespace Stubble.Core
{
    public class Parser
    {
        #region Static Regex
        private static readonly Regex WhitespaceRegex = new Regex(@"\s*", RegexOptions.Compiled);
        private static readonly Regex SpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex EqualsRegex = new Regex(@"\s*=", RegexOptions.Compiled);
        private static readonly Regex CurlyRegex = new Regex(@"\s*\}", RegexOptions.Compiled);
        private static readonly Regex TagRegex = new Regex(@"#|\^|\/|>|\{|&|=|!", RegexOptions.Compiled);
        private static readonly Regex EscapeRegex = new Regex(@"[\-\[\]{}()*+?.,\^$|#\s]", RegexOptions.Compiled);

        internal static readonly ConcurrentDictionary<string, TagRegexes> TagRegexCache = new ConcurrentDictionary<string, TagRegexes>(
            new Dictionary<string, TagRegexes>
            {
                { "{{ }}", new TagRegexes()
                    {
                        OpenTag = new Regex(@"\{\{\s*"),
                        CloseTag = new Regex(@"\s*\}\}"),
                        ClosingTag = new Regex(@"\s*\}\}\}")
                    }
                }
            });

        internal struct TagRegexes
        {
            internal Regex OpenTag { get; set; }
            internal Regex CloseTag { get; set; }
            internal Regex ClosingTag { get; set; }
        }

        #endregion

        private Regex _openingTagRegex;
        private Regex _closingTagRegex;
        private Regex _closingCurlyRegex;
        private Tags _currentTags;
        public static readonly Tags DefaultTags = new Tags("{{", "}}");

        public IList<ParserOutput> ParseTemplate(string template)
        {
            return ParseTemplate(template, null);
        }

        public IList<ParserOutput> ParseTemplate(string template, Tags tags)
        {
            if (string.IsNullOrEmpty(template))
                return new List<ParserOutput>();

            CompileTags(tags ?? DefaultTags);

            var scanner = new Scanner(template);
            var sections = new Stack<ParserOutput>();
            ParserOutput openSection;
            var tokens = new List<ParserOutput>();
            var spaces = new Stack<int>();
            var hasTag = false;
            var nonSpace = false;
            while (!scanner.EOS)
            {
                var start = scanner.Pos;

                var value = scanner.ScanUntil(_openingTagRegex);

                if (!string.IsNullOrEmpty(value))
                {
                    foreach (var c in value)
                    {
                        if (char.IsWhiteSpace(c))
                        {
                            spaces.Push(tokens.Count);
                        }
                        else
                        {
                            nonSpace = true;
                        }
                        tokens.Add(new RawValueToken() { TokenType = "text", Value = c.ToString(), Start = start, End = start + 1 });
                        start += 1;

                        if (c != '\n') continue;
                        if (hasTag && !nonSpace)
                        {
                            while (spaces.Count > 0)
                            {
                                tokens.RemoveAt(spaces.Pop());
                            }
                        }
                        else
                        {
                            spaces = new Stack<int>();
                        }

                        hasTag = false;
                        nonSpace = false;
                    }
                }

                if (string.IsNullOrEmpty(scanner.Scan(_openingTagRegex)))
                    break;

                hasTag = true;

                var type = scanner.Scan(TagRegex);
                type = string.IsNullOrEmpty(type) ? "name" : type;
                scanner.Scan(WhitespaceRegex);

                switch (type)
                {
                    case "=":
                        value = scanner.ScanUntil(EqualsRegex);
                        scanner.Scan(EqualsRegex);
                        scanner.ScanUntil(_closingTagRegex);
                        break;
                    case "{":
                        value = scanner.ScanUntil(_closingCurlyRegex);
                        scanner.Scan(CurlyRegex);
                        scanner.ScanUntil(_closingTagRegex);
                        type = "&";
                        break;
                    default:
                        value = scanner.ScanUntil(_closingTagRegex);
                        break;
                }

                if (string.IsNullOrEmpty(scanner.Scan(_closingTagRegex)))
                {
                    throw new Exception("Unclosed Tag at " + scanner.Pos);
                }

                var token = GetCorrectTypedToken(type, _currentTags);
                token.Value = value;
                token.Start = start;
                token.End = scanner.Pos;
                tokens.Add(token);

                switch (type)
                {
                    case "#":
                    case "^":
                        sections.Push(token);
                        break;
                    case "/":
                        if (sections.Count == 0)
                        {
                            throw new Exception("Unopened Section '" + value + "' at " + start);
                        }
                        openSection = sections.Pop();

                        if (openSection.Value != token.Value)
                        {
                            throw new Exception("Unclosed Section '" + openSection.Value + "' at " + start);
                        }
                        break;
                    case "name":
                    case "{":
                    case "&":
                        nonSpace = true;
                        break;
                    case "=":
                        CompileTags(value);
                        break;
                }
            }

            // Make sure there are no open sections when we're done.
            if (sections.Count > 0)
            {
                openSection = sections.Pop();
                throw new Exception("Unclosed Section '" + openSection.Value + "' at " + scanner.Pos);
            }

            return NestTokens(SquishTokens(tokens));
        }

        private static IEnumerable<ParserOutput> SquishTokens(IEnumerable<ParserOutput> tokens)
        {
            using (var iterator = tokens.GetEnumerator())
            {
                RawValueToken lastItem = null;
                while (iterator.MoveNext())
                {
                    var item = iterator.Current;
                    if (lastItem != null && item is RawValueToken)
                    {
                        lastItem.ValueBuilder.Append(item.Value);
                        lastItem.End = item.End;
                        continue;
                    }
                    lastItem = item as RawValueToken;
                    yield return item;
                }
            }
        }

        private static IList<ParserOutput> NestTokens(IEnumerable<ParserOutput> tokens)
        {
            var nestedTokens = new List<ParserOutput>();
            var collector = nestedTokens;
            var sections = new Stack<ParserOutput>();

            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case "#":
                    case "^":
                        collector.Add(token);
                        sections.Push(token);
                        collector = token.ChildTokens = new List<ParserOutput>();
                        break;
                    case "/":
                        var section = sections.Pop();
                        section.ParentSectionEnd = token.Start;
                        collector = sections.Count > 0 ? sections.Peek().ChildTokens : nestedTokens;
                        break;
                    default:
                        collector.Add(token);
                        break;
                }
            }

            return nestedTokens;
        }

        private void CompileTags(string value)
        {
            CompileTags(new Tags(SpaceRegex.Split(value)));
        }

        private void CompileTags(Tags tags)
        {
            _currentTags = tags;
            TagRegexes tagRegexes;
            var tagString = tags.ToString();
            if (!TagRegexCache.TryGetValue(tagString, out tagRegexes))
            {
                tagRegexes = new TagRegexes()
                {
                    OpenTag = new Regex(EscapeRegexExpression(tags.StartTag) + @"\s*"),
                    CloseTag = new Regex(@"\s*" + EscapeRegexExpression(tags.EndTag)),
                    ClosingTag = new Regex(@"\s*" + EscapeRegexExpression("}" + tags.EndTag))
                };
                AddToRegexCache(tagString, tagRegexes);
            }

            _openingTagRegex = tagRegexes.OpenTag;
            _closingTagRegex = tagRegexes.CloseTag;
            _closingCurlyRegex = tagRegexes.ClosingTag;
        }

        private static string EscapeRegexExpression(string expression)
        {
            return EscapeRegex.Replace(expression, @"\$&");
        }

        private static ParserOutput GetCorrectTypedToken(string tokenType, Tags currentTags)
        {
            switch (tokenType)
            {
                case "#":
                    return new SectionToken { TokenType = tokenType, Tags = currentTags };
                case "^":
                    return new InvertedToken { TokenType = tokenType };
                case ">":
                    return new PartialToken { TokenType = tokenType };
                case "&":
                    return new UnescapedValueToken { TokenType = tokenType };
                case "name":
                    return new EscapedValueToken { TokenType = tokenType };
                case "text":
                    return new RawValueToken { TokenType = tokenType };
                default:
                    return new ParserOutput { TokenType = tokenType };
            }
        }

        private static void AddToRegexCache(string dictionaryKey, TagRegexes regex)
        {
            if (TagRegexCache.Count >= 4)
            {
                TagRegexes outValue;
                TagRegexCache.TryRemove(TagRegexCache.Last().Key, out outValue);
            }

            TagRegexCache.AddOrUpdate(dictionaryKey, regex, (key, existingVal) => regex);
        }
    }
}
