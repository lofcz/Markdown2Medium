namespace MarkdownToMedium.Demo;

class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        string input = await File.ReadAllTextAsync("article.md");
        string output = MarkdownToMediumConverter.Convert(input, InlineCodeFormat.Italic);

        await File.WriteAllTextAsync("output.html", output);
    }
}