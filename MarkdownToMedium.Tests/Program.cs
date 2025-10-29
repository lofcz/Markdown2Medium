using MarkdownToMedium;

Console.WriteLine("=== MarkdownToMedium Converter Test ===\n");

// Test 1: Basic markdown
string markdown1 = "# Hello World\n\nThis is **bold** and this is *italic*.";
string html1 = MarkdownToMediumConverter.Convert(markdown1);
Console.WriteLine("Test 1 - Basic Markdown:");
Console.WriteLine($"Input: {markdown1}");
Console.WriteLine($"Output: {html1}\n");

// Test 2: Inline code with different formats
string markdown2 = "Use the `Console.WriteLine()` method.";

Console.WriteLine("Test 2 - Inline Code Formatting:");
Console.WriteLine($"Input: {markdown2}");

Console.WriteLine("  - DoubleQuotes: " + MarkdownToMediumConverter.Convert(markdown2, InlineCodeFormat.DoubleQuotes));
Console.WriteLine("  - Bold: " + MarkdownToMediumConverter.Convert(markdown2, InlineCodeFormat.Bold));
Console.WriteLine("  - Italic: " + MarkdownToMediumConverter.Convert(markdown2, InlineCodeFormat.Italic));
Console.WriteLine("  - BoldAndItalic: " + MarkdownToMediumConverter.Convert(markdown2, InlineCodeFormat.BoldAndItalic));
Console.WriteLine("  - BoldWithQuotes: " + MarkdownToMediumConverter.Convert(markdown2, InlineCodeFormat.BoldWithQuotes));
Console.WriteLine("  - ItalicWithQuotes: " + MarkdownToMediumConverter.Convert(markdown2, InlineCodeFormat.ItalicWithQuotes));
Console.WriteLine("  - All: " + MarkdownToMediumConverter.Convert(markdown2, InlineCodeFormat.All));
Console.WriteLine();

// Test 3: Lists
string markdown3 = @"## My List

- Item 1
- Item 2
- Item 3

1. First
2. Second
3. Third";

string html3 = MarkdownToMediumConverter.Convert(markdown3);
Console.WriteLine("Test 3 - Lists:");
Console.WriteLine($"Output:\n{html3}\n");

// Test 4: Table
string markdown4 = @"| Name | Age |
|------|-----|
| John | 30  |
| Jane | 25  |";

string html4 = MarkdownToMediumConverter.Convert(markdown4);
Console.WriteLine("Test 4 - Table:");
Console.WriteLine($"Output:\n{html4}\n");

// Test 5: Links and images
string markdown5 = "Visit [my website](https://example.com) and see ![alt text](https://example.com/image.png)";
string html5 = MarkdownToMediumConverter.Convert(markdown5);
Console.WriteLine("Test 5 - Links and Images:");
Console.WriteLine($"Output: {html5}\n");

// Test 6: Line breaks
string markdown6 = "Line one\nLine two\nLine three";
string html6 = MarkdownToMediumConverter.Convert(markdown6);
Console.WriteLine("Test 6 - Line Breaks:");
Console.WriteLine($"Output: {html6}\n");

// Test 7: Empty and null handling
string html7 = MarkdownToMediumConverter.Convert("");
Console.WriteLine("Test 7 - Empty String:");
Console.WriteLine($"Output: '{html7}'\n");

try
{
    MarkdownToMediumConverter.Convert(null!);
}
catch (ArgumentNullException)
{
    Console.WriteLine("Test 8 - Null Input: Correctly threw ArgumentNullException\n");
}

Console.WriteLine("=== All Tests Complete ===");
