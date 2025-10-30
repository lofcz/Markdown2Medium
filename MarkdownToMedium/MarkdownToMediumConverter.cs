using System;
using System.IO;
using Markdig;
using Markdig.Syntax;

namespace MarkdownToMedium;

/// <summary>
/// Provides a static method to convert Markdown to Medium-compatible HTML.
/// </summary>
public static class MarkdownToMediumConverter
{
    private static readonly MarkdownPipeline defaultPipeline = BuildPipeline();

    /// <summary>
    /// Converts Markdown text to Medium-compatible HTML.
    /// </summary>
    /// <param name="markdown">The Markdown text to convert.</param>
    /// <param name="inlineCodeFormat">
    /// The formatting style to apply to inline code spans. Defaults to <see cref="InlineCodeFormat.DoubleQuotes"/>.
    /// </param>
    /// <returns>A string containing the converted HTML.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="markdown"/> is null.</exception>
    public static string Convert(string markdown, InlineCodeFormat inlineCodeFormat = InlineCodeFormat.DoubleQuotes)
    {
        if (markdown == null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        using StringWriter writer = new StringWriter();
        MediumHtmlRenderer renderer = new MediumHtmlRenderer(writer, inlineCodeFormat);
        defaultPipeline.Setup(renderer);
        
        // Setup custom renderers AFTER pipeline setup to ensure they override any defaults
        renderer.SetupCustomRenderers();

        MarkdownDocument document = Markdown.Parse(markdown, defaultPipeline);
        renderer.Render(document);
        writer.Flush();

        return writer.ToString();
    }

    /// <summary>
    /// Builds the Markdig pipeline with settings matching the original JavaScript implementation.
    /// </summary>
    private static MarkdownPipeline BuildPipeline()
    {
        return new MarkdownPipelineBuilder()
            .UseEmphasisExtras()          // Bold, italic, strikethrough
            .UsePipeTables()              // Enables table support
            .UseTaskLists()               // GitHub task lists
            .UseAutoLinks()               // Auto-convert URLs to links
            .UseListExtras()              // Smart list handling
            .UseFootnotes()               // Footnote support
            .UseAutoIdentifiers()         // Auto-generate heading IDs
            .UseSoftlineBreakAsHardlineBreak()  // Converts line breaks to <br> tags
            .DisableHtml()                // Sanitize HTML (matches original sanitize: true)
            .Build();
    }
}

