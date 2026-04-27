using NetHexaGen.Commands;
using Spectre.Console.Cli;

namespace NetHexaGen;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandApp();
        
        app.Configure(config =>
        {
            config.SetApplicationName("nethexagen");
            
            config.AddCommand<NewCommand>("new")
                .WithDescription("Generates a new Clean Architecture project with Enterprise Standards.")
                .WithExample(new[] { "new", "MyVortexApp", "--api-type", "minimal" });

            config.AddCommand<AddResourceCommand>("add")
                .WithDescription("Adds a new resource (Entity, Handlers, Controller) to an existing project.");
        });

        return app.Run(args);
    }
}
