using System.ComponentModel;
using NetHexaGen.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NetHexaGen.Commands;

public sealed class NewCommand : AsyncCommand<NewCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<PROJECT_NAME>")]
        [Description("The name of the project to create.")]
        public string ProjectName { get; init; } = string.Empty;

        [CommandOption("-o|--output")]
        [Description("The output directory.")]
        public string? Output { get; init; }

        [CommandOption("--api-type")]
        [Description("The type of API to generate (minimal|controllers).")]
        [DefaultValue("minimal")]
        public string ApiType { get; init; } = "minimal";
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        ConsoleHelper.ShowBanner();
        
        var apiType = settings.ApiType;
        
        // Interactive prompt if it's the default or not specified
        if (string.IsNullOrEmpty(apiType) || apiType == "minimal") 
        {
            apiType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What type of [green]API[/] would you like to generate?")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Controllers", "Minimal APIs"
                    }));
            
            // Normalize for the generator
            apiType = apiType.Contains("Controllers") ? "controllers" : "minimal";
        }

        // Database selection prompt
        var dbProvider = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which [blue]Database Provider[/] will you use?")
                .AddChoices(new[] { "SQL Server", "PostgreSQL", "SQLite" }));

        // Normalize DB provider
        dbProvider = dbProvider switch
        {
            "PostgreSQL" => "postgresql",
            "SQLite" => "sqlite",
            _ => "sqlserver"
        };

        var engine = new Core.TemplateEngine();
        var generator = new Core.ProjectGenerator(engine);
        var output = settings.Output ?? Directory.GetCurrentDirectory();

        await AnsiConsole.Status()
            .StartAsync($"Creating project [bold yellow]{settings.ProjectName}[/]...", async ctx => 
            {
                ctx.Status("Scaffolding Clean Architecture layers...");
                await generator.GenerateAsync(settings.ProjectName, output, apiType, dbProvider);
                
                ctx.Status("Adding Enterprise Standards (CQRS, DDD, Results)...");
                await Task.Delay(500);

                ctx.Status($"Configuring {settings.ApiType} API...");
                await Task.Delay(500);
            });

        ConsoleHelper.ShowSuccess($"Project [bold white]{settings.ProjectName}[/] created successfully!");
        AnsiConsole.MarkupLine($"[blue]Location:[/] {Path.Combine(output, settings.ProjectName)}");
        AnsiConsole.MarkupLine($"[blue]Type:[/] {settings.ApiType}");
        AnsiConsole.MarkupLine($"[blue]Target:[/] .NET 10");
        
        return 0;
    }
}
