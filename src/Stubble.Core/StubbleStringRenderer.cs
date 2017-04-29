﻿// <copyright file="StubbleStringRenderer.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Stubble.Core.Classes;
using Stubble.Core.Classes.Exceptions;
using Stubble.Core.Dev.Settings;
using Stubble.Core.Interfaces;

namespace Stubble.Core
{
    /// <summary>
    /// Represents the core StubbleStringRenderer which renders Mustache templates
    /// </summary>
    public sealed class StubbleStringRenderer : IStubbleStringRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubbleStringRenderer"/> class
        /// with a default <see cref="Registry"/>
        /// </summary>
        public StubbleStringRenderer()
            : this(new RendererSettingsBuilder().BuildSettings(), new Registry())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StubbleStringRenderer"/> class
        /// with a passed Registry
        /// </summary>
        /// <param name="rendererSettings">The renderer settings for the renderer</param>
        /// <param name="registry">A registry instance</param>
        internal StubbleStringRenderer(RendererSettings rendererSettings, Registry registry)
        {
            RendererSettings = rendererSettings;
            Parser = new Parser(registry.TokenMatchRegex, registry.TokenGetters);
            Writer = new Writer(RendererSettings, Parser);
        }

        /// <summary>
        /// Gets the renderer settings for the renderer
        /// </summary>
        internal RendererSettings RendererSettings { get; }

        /// <summary>
        /// Gets the core Writer instance for the Renderer
        /// </summary>
        internal Writer Writer { get; }

        /// <summary>
        /// Gets the core Parser instance for the Renderer
        /// </summary>
        internal Parser Parser { get; }

        /// <summary>
        /// Renders the template with the given view using the writer.
        /// </summary>
        /// <param name="template">The mustache teplate to render</param>
        /// <param name="view">The data to use for rendering</param>
        /// <returns>A mustache rendered string</returns>
        public string Render(string template, object view)
        {
            return Render(template, view, null, null);
        }

        /// <summary>
        /// Renders the template with the given view using the writer
        /// and the given render settings.
        /// </summary>
        /// <param name="template">The mustache teplate to render</param>
        /// <param name="view">The data to use for rendering</param>
        /// <param name="settings">Any settings you wish to override the defaults with</param>
        /// <returns>A mustache rendered string</returns>
        public string Render(string template, object view, RenderSettings settings)
        {
            return Render(template, view, null, settings);
        }

        /// <summary>
        /// Renders the template with the given view and partials using
        /// the writer.
        /// </summary>
        /// <param name="template">The mustache teplate to render</param>
        /// <param name="view">The data to use for rendering</param>
        /// <param name="partials">A hash of Partials</param>
        /// <returns>A mustache rendered string</returns>
        public string Render(string template, object view, IDictionary<string, string> partials)
        {
            return Render(template, view, partials, null);
        }

        /// <summary>
        /// Renders the template with the given view and partials using
        /// the writer and the given Render Settings
        /// </summary>
        /// <param name="template">The mustache teplate to render</param>
        /// <param name="view">The data to use for rendering</param>
        /// <param name="partials">A hash of Partials</param>
        /// <param name="settings">Any settings you wish to override the defaults with</param>
        /// <returns>A mustache rendered string</returns>
        public string Render(string template, object view, IDictionary<string, string> partials, RenderSettings settings)
        {
            var loadedTemplate = RendererSettings.TemplateLoader.Load(template);

            if (loadedTemplate == null)
            {
                throw new UnknownTemplateException("No template was found with the name '" + template + "'");
            }

            var tokens = Parser.Parse(loadedTemplate);

            return Writer.Render(loadedTemplate, tokens, new Context(view, RendererSettings, settings ?? RendererSettings.RenderSettings), partials);
        }

        /// <summary>
        /// Parses and caches the given template in the writer and returns the list
        /// of tokens it contains. Doing this ahead of time avoids the need to parse
        /// templates on the fly as they are rendered.
        ///
        /// If you don't need the result <see cref="CacheTemplate(string)"/>
        /// </summary>
        /// <param name="template">The mustache teplate to parse</param>
        /// <returns>Returns a list of tokens</returns>
        public IList<ParserOutput> Parse(string template)
        {
            return Parse(template, (Tags)null);
        }

        /// <summary>
        /// Parses and caches the given template in the writer and returns the list
        /// of tokens it contains. Doing this ahead of time avoids the need to parse
        /// templates on the fly as they are rendered.
        ///
        /// If you don't need the result <see cref="CacheTemplate(string,Tags)"/>
        /// </summary>
        /// <param name="template">The mustache teplate to parse</param>
        /// <param name="tags">The set of tags to use for parsing</param>
        /// <returns>Returns a list of tokens</returns>
        public IList<ParserOutput> Parse(string template, Tags tags)
        {
            var loadedTemplate = RendererSettings.TemplateLoader.Load(template);
            return Parser.Parse(loadedTemplate, tags);
        }

        /// <summary>
        /// Parses and caches the given template in the writer and returns the list
        /// of tokens it contains. Doing this ahead of time avoids the need to parse
        /// templates on the fly as they are rendered.
        ///
        /// If you don't need the result <see cref="CacheTemplate(string,string)"/>
        /// </summary>
        /// <param name="template">The mustache teplate to parse</param>
        /// <param name="tags">A tag string split by a space e.g. {{ }}</param>
        /// <returns>Returns a list of tokens</returns>
        public IList<ParserOutput> Parse(string template, string tags)
        {
            return Parse(template, new Tags(tags.Split(' ')));
        }

        /// <summary>
        /// Parses a template and adds the result to the writer cache.
        /// </summary>
        /// <param name="template">The mustache teplate to parse</param>
        public void CacheTemplate(string template)
        {
            Parse(template);
        }

        /// <summary>
        /// Parses a template with given tags and adds the result to the writer cache.
        /// </summary>
        /// <param name="template">The mustache teplate to parse</param>
        /// <param name="tags">The set of tags to use for parsing</param>
        public void CacheTemplate(string template, Tags tags)
        {
            Parse(template, tags);
        }

        /// <summary>
        /// Parses a template with given tags and adds the result to the writer cache.
        /// </summary>
        /// <param name="template">The mustache teplate to parse</param>
        /// <param name="tags">A tag string split by a space e.g. {{ }}</param>
        public void CacheTemplate(string template, string tags)
        {
            Parse(template, tags);
        }

        /// <summary>
        /// Clears all cached templates in the Writer.
        /// </summary>
        public void ClearCache()
        {
            Parser.Cache.Clear();
        }
    }
}
