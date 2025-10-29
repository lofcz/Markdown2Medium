namespace MarkdownToMedium;

/// <summary>
/// Specifies how inline code spans should be formatted in the output HTML.
/// </summary>
public enum InlineCodeFormat
{
    /// <summary>
    /// Format inline code as bold text.
    /// </summary>
    Bold,

    /// <summary>
    /// Format inline code as italic text.
    /// </summary>
    Italic,

    /// <summary>
    /// Format inline code with double quotes around it.
    /// </summary>
    DoubleQuotes,

    /// <summary>
    /// Format inline code as bold and italic text.
    /// </summary>
    BoldAndItalic,

    /// <summary>
    /// Format inline code as bold text with double quotes.
    /// </summary>
    BoldWithQuotes,

    /// <summary>
    /// Format inline code as italic text with double quotes.
    /// </summary>
    ItalicWithQuotes,

    /// <summary>
    /// Format inline code as bold, italic text with double quotes.
    /// </summary>
    All
}

