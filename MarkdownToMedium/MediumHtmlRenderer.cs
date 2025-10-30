using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownToMedium;

/// <summary>
/// Custom HTML renderer for Medium-compatible output with configurable inline code formatting.
/// </summary>
internal sealed class MediumHtmlRenderer : HtmlRenderer
{
    private readonly InlineCodeFormat _inlineCodeFormat;

    public MediumHtmlRenderer(TextWriter writer, InlineCodeFormat inlineCodeFormat) : base(writer)
    {
        _inlineCodeFormat = inlineCodeFormat;
    }
    
    /// <summary>
    /// Called to setup the renderer with custom renderers after the pipeline has been setup.
    /// This must be called after pipeline.Setup(renderer) to ensure our custom renderers override the defaults.
    /// </summary>
    public void SetupCustomRenderers()
    {
        // Replace the default code inline renderer with our custom one
        var codeInlineRenderer = ObjectRenderers.FindExact<CodeInlineRenderer>();
        if (codeInlineRenderer != null)
        {
            ObjectRenderers.Remove(codeInlineRenderer);
        }
        ObjectRenderers.Add(new MediumCodeInlineRenderer(_inlineCodeFormat));
        
        // Replace the default CodeBlockRenderer with our custom Medium-compatible one
        var defaultCodeBlockRenderer = ObjectRenderers.FindExact<Markdig.Renderers.Html.CodeBlockRenderer>();
        if (defaultCodeBlockRenderer != null)
        {
            ObjectRenderers.Remove(defaultCodeBlockRenderer);
        }
        
        // Add our custom renderers for both CodeBlock and FencedCodeBlock
        // NOTE: We add the more specific FencedCodeBlock renderer first, then the general CodeBlock renderer
        // Markdig will use the most specific renderer that matches
        ObjectRenderers.Add(new MediumFencedCodeBlockRenderer());
        ObjectRenderers.Add(new MediumCodeBlockRenderer());
        
        // Replace the default table renderer with our custom ASCII table renderer
        var tableRenderer = ObjectRenderers.FindExact<HtmlTableRenderer>();
        if (tableRenderer != null)
        {
            ObjectRenderers.Remove(tableRenderer);
        }
        ObjectRenderers.Add(new AsciiTableRenderer());
    }

    /// <summary>
    /// Shared rendering logic for both code blocks and fenced code blocks.
    /// </summary>
    private static void RenderMediumCodeBlock(HtmlRenderer renderer, CodeBlock obj)
    {
        renderer.EnsureLine();
        
        // Get the code content
        string code = obj.Lines.ToString();
        
        if (string.IsNullOrEmpty(code))
        {
            renderer.Write("<pre></pre>");
            renderer.WriteLine();
            return;
        }
        
        // Split into lines
        var lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.None)
            .Where(l => l.Length > 0 || code.EndsWith("\n")) // Keep empty lines that were in original
            .ToList();
        
        // Remove trailing empty line if present (common artifact)
        if (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }
        
        renderer.Write("<pre>");
        
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            
            // Empty lines become &nbsp;
            if (string.IsNullOrWhiteSpace(line))
            {
                renderer.Write("&nbsp;");
            }
            else
            {
                // Escape HTML entities
                renderer.WriteEscape(line);
            }
            
            // Add <br> between lines (but not after the last line)
            if (i < lines.Count - 1)
            {
                renderer.Write("<br>");
            }
        }
        
        renderer.Write("</pre>");
        renderer.WriteLine();
    }
    
    /// <summary>
    /// Custom renderer for fenced code blocks that outputs Medium-compatible format.
    /// Medium requires PRE with BR tags instead of PRE with CODE tags.
    /// </summary>
    private sealed class MediumFencedCodeBlockRenderer : HtmlObjectRenderer<FencedCodeBlock>
    {
        protected override void Write(HtmlRenderer renderer, FencedCodeBlock obj)
        {
            RenderMediumCodeBlock(renderer, obj);
        }
    }
    
    /// <summary>
    /// Custom renderer for code blocks that outputs Medium-compatible format.
    /// Medium requires PRE with BR tags instead of PRE with CODE tags.
    /// </summary>
    private sealed class MediumCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
    {
        protected override void Write(HtmlRenderer renderer, CodeBlock obj)
        {
            RenderMediumCodeBlock(renderer, obj);
        }
    }
    
    /// <summary>
    /// Custom renderer for inline code that applies Medium-specific formatting.
    /// </summary>
    private sealed class MediumCodeInlineRenderer : HtmlObjectRenderer<CodeInline>
    {
        private readonly InlineCodeFormat _format;

        public MediumCodeInlineRenderer(InlineCodeFormat format)
        {
            _format = format;
        }

        protected override void Write(HtmlRenderer renderer, CodeInline obj)
        {
            var content = obj.Content;
            
            // Apply formatting based on the selected format
            switch (_format)
            {
                case InlineCodeFormat.Bold:
                    renderer.Write("<strong>").WriteEscape(content).Write("</strong>");
                    break;
                
                case InlineCodeFormat.Italic:
                    renderer.Write("<em>").WriteEscape(content).Write("</em>");
                    break;
                
                case InlineCodeFormat.DoubleQuotes:
                    renderer.Write("&quot;").WriteEscape(content).Write("&quot;");
                    break;
                
                case InlineCodeFormat.BoldAndItalic:
                    renderer.Write("<strong><em>").WriteEscape(content).Write("</em></strong>");
                    break;
                
                case InlineCodeFormat.BoldWithQuotes:
                    renderer.Write("<strong>&quot;").WriteEscape(content).Write("&quot;</strong>");
                    break;
                
                case InlineCodeFormat.ItalicWithQuotes:
                    renderer.Write("<em>&quot;").WriteEscape(content).Write("&quot;</em>");
                    break;
                
                case InlineCodeFormat.All:
                    renderer.Write("<strong><em>&quot;").WriteEscape(content).Write("&quot;</em></strong>");
                    break;
                
                default:
                    // Fallback to double quotes
                    renderer.Write("&quot;").WriteEscape(content).Write("&quot;");
                    break;
            }
        }
    }

    /// <summary>
    /// Custom renderer that converts Markdown tables to formatted text.
    /// Medium doesn't support HTML tables, so we convert them to readable text format.
    /// </summary>
    private sealed class AsciiTableRenderer : HtmlObjectRenderer<Table>
    {
        protected override void Write(HtmlRenderer renderer, Table table)
        {
            var rows = new List<List<string>>();
            int headerRowCount = 0;

            // Extract all cell contents
            foreach (var rowObj in table)
            {
                if (rowObj is TableRow row)
                {
                    var cells = new List<string>();
                    
                    foreach (var cellObj in row)
                    {
                        if (cellObj is TableCell cell)
                        {
                            var cellContent = ExtractCellContent(cell);
                            cells.Add(cellContent);
                        }
                    }
                    
                    rows.Add(cells);
                    
                    if (row.IsHeader)
                    {
                        headerRowCount++;
                    }
                }
            }

            if (rows.Count == 0)
            {
                return;
            }

            // Build Markdown-style pipe table with proper emoji width handling
            var tableText = new StringBuilder();
            int columnCount = rows.Max(r => r.Count);

            // Calculate column widths based on visual/display width
            var columnWidths = new int[columnCount];
            for (int col = 0; col < columnCount; col++)
            {
                columnWidths[col] = rows
                    .Where(r => col < r.Count)
                    .Select(r => GetDisplayWidth(r[col]))
                    .Max();
                columnWidths[col] = Math.Max(3, columnWidths[col]);
            }

            // Render rows
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                
                // Data row
                tableText.Append("| ");
                for (int col = 0; col < columnCount; col++)
                {
                    var cellValue = col < row.Count ? row[col] : "";
                    var displayWidth = GetDisplayWidth(cellValue);
                    var paddingNeeded = columnWidths[col] - displayWidth;
                    
                    tableText.Append(cellValue);
                    tableText.Append(new string(' ', Math.Max(0, paddingNeeded)));
                    tableText.Append(" | ");
                }
                tableText.AppendLine();
                
                // Separator after header row
                if (rowIndex == headerRowCount - 1 && rowIndex < rows.Count - 1)
                {
                    tableText.Append("|");
                    for (int col = 0; col < columnCount; col++)
                    {
                        tableText.Append(new string('-', columnWidths[col] + 2));
                        tableText.Append("|");
                    }
                    tableText.AppendLine();
                }
            }

            // Wrap in <pre> block with <br> tags for Medium (same format as code blocks)
            string tableContent = tableText.ToString().TrimEnd();
            var lines = tableContent.Split(new[] { '\n', '\r' }, StringSplitOptions.None)
                .Where(l => l.Length > 0)
                .ToList();
            
            // Remove trailing empty line if present
            if (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
            {
                lines.RemoveAt(lines.Count - 1);
            }
            
            renderer.Write("<pre>");
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                
                // Empty lines become &nbsp;
                if (string.IsNullOrWhiteSpace(line))
                {
                    renderer.Write("&nbsp;");
                }
                else
                {
                    // Escape HTML entities
                    renderer.WriteEscape(line);
                }
                
                // Add <br> between lines (but not after the last line)
                if (i < lines.Count - 1)
                {
                    renderer.Write("<br>");
                }
            }
            renderer.Write("</pre>");
            renderer.WriteLine();
        }

        /// <summary>
        /// Calculates the display width of a string, accounting for emojis and wide characters.
        /// Emojis are typically rendered as double-width in monospace fonts.
        /// </summary>
        private int GetDisplayWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int width = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                
                // Skip variation selectors and zero-width characters (these don't add width)
                if (c == 0xFE0F || c == 0xFE0E || c == 0x200D || (c >= 0x180B && c <= 0x180D))
                {
                    continue;
                }
                
                // Surrogate pairs (most emojis)
                if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    width += 2;
                    i++; // Skip the low surrogate
                }
                // Common emoji ranges in Basic Multilingual Plane
                else if ((c >= 0x2600 && c <= 0x27BF) ||  // Miscellaneous Symbols (✅, ⚠️, etc.)
                         (c >= 0x2300 && c <= 0x23FF) ||  // Miscellaneous Technical
                         (c >= 0x2B00 && c <= 0x2BFF) ||  // Miscellaneous Symbols and Arrows
                         (c >= 0x1F300 && c <= 0x1F9FF))  // Emoticons
                {
                    width += 2;
                }
                // Regular ASCII and most characters
                else
                {
                    width += 1;
                }
            }
            
            return width;
        }

        /// <summary>
        /// Extracts plain text content from a table cell.
        /// </summary>
        private string ExtractCellContent(TableCell cell)
        {
            var content = new StringBuilder();
            
            foreach (var item in cell)
            {
                if (item is ParagraphBlock paragraph)
                {
                    foreach (var inline in paragraph.Inline ?? Enumerable.Empty<Inline>())
                    {
                        ExtractInlineContent(inline, content);
                    }
                }
                else if (item is LeafBlock leaf && leaf.Inline != null)
                {
                    foreach (var inline in leaf.Inline)
                    {
                        ExtractInlineContent(inline, content);
                    }
                }
            }
            
            return content.ToString().Trim();
        }

        /// <summary>
        /// Recursively extracts text from inline elements.
        /// </summary>
        private void ExtractInlineContent(Inline inline, StringBuilder content)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    content.Append(literal.Content.ToString());
                    break;
                    
                case CodeInline code:
                    content.Append(code.Content);
                    break;
                    
                case LineBreakInline:
                    content.Append(" ");
                    break;
                    
                case EmphasisInline emphasis:
                    foreach (var child in emphasis)
                    {
                        ExtractInlineContent(child, content);
                    }
                    break;
                    
                case LinkInline link:
                    foreach (var child in link)
                    {
                        ExtractInlineContent(child, content);
                    }
                    break;
                    
                case ContainerInline container:
                    foreach (var child in container)
                    {
                        ExtractInlineContent(child, content);
                    }
                    break;
            }
        }
    }
}

