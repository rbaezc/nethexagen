using System.ComponentModel;
using NetHexaGen.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NetHexaGen.Commands;

public sealed class AddResourceCommand : AsyncCommand<AddResourceCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<RESOURCE_NAME>")]
        [Description("The name of the resource (entity) to add.")]
        public string ResourceName { get; init; } = string.Empty;

        [CommandOption("-p|--project")]
        [Description("The name of the existing project solution.")]
        public string? ProjectName { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        ConsoleHelper.ShowBanner();

        var projectName = settings.ProjectName;
        
        // If ProjectName is not provided, try to find it from the current directory .slnx file
        if (string.IsNullOrEmpty(projectName))
        {
            var slnFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.slnx").FirstOrDefault();
            if (slnFile != null)
            {
                projectName = Path.GetFileNameWithoutExtension(slnFile);
            }
        }

        if (string.IsNullOrEmpty(projectName))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Could not determine project name. Please provide it via [yellow]--project[/] or run this command from the project root.");
            return 1;
        }

        var engine = new Core.TemplateEngine();
        var generator = new Core.ProjectGenerator(engine); // We'll extend this or use a new one

        await AnsiConsole.Status()
            .StartAsync($"Adding resource [bold yellow]{settings.ResourceName}[/] to [bold blue]{projectName}[/]...", async ctx => 
            {
                var output = Directory.GetCurrentDirectory();
                
                // Detect ApiType
                var apiPath = Path.Combine(output, $"{projectName}.Api");
                var apiType = Directory.Exists(Path.Combine(apiPath, "Controllers")) ? "controllers" : "minimal";

                await generator.GenerateResourceAsync(projectName, output, settings.ResourceName, apiType);
                
                ctx.Status("Registering entity in Domain...");
                await Task.Delay(300);
                
                ctx.Status("Creating application handlers...");
                await Task.Delay(300);
                
                ctx.Status("Adding API controller...");
                await Task.Delay(300);
            });

        ConsoleHelper.ShowSuccess($"Resource [bold white]{settings.ResourceName}[/] added successfully across all layers!");
        
        return 0;
    }
}
