using Spectre.Console;

namespace NetHexaGen.Infrastructure;

public static class ConsoleHelper
{
    public static void ShowBanner()
    {
        AnsiConsole.Write(
            new FigletText("NetHexaGen")
                .Centered()
                .Color(Color.Purple));

        var rule = new Rule("[gray]Vortex Solutions - Enterprise Scaffolding for .NET 10[/]");
        rule.Style = Style.Parse("purple dim");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public static void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[bold green]SUCCESS:[/] {message}");
    }

    public static void ShowHeader(string title)
    {
        AnsiConsole.Write(new Rule($"[yellow]{title}[/]").RuleStyle("grey"));
    }
}
