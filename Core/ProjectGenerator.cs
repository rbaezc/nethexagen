using System.Text;
using System.Diagnostics;
using NetHexaGen.Infrastructure;

namespace NetHexaGen.Core;

public class ProjectGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string _templatePath;

    public ProjectGenerator(TemplateEngine engine)
    {
        _engine = engine;
        _templatePath = Path.Combine(AppContext.BaseDirectory, "Templates");
        
        // Fallback if running from source
        if (!Directory.Exists(_templatePath))
        {
             _templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        }
    }

    public async Task GenerateAsync(string projectName, string outputDir, string apiType, string dbProvider)
    {
        var rootDir = Path.Combine(outputDir, projectName);
        Directory.CreateDirectory(rootDir);

        var model = new { ProjectName = projectName, ApiType = apiType, DbProvider = dbProvider };

        // 1. Generate Domain Layer
        await GenerateDomainLayer(rootDir, projectName, model);

        // 2. Generate Application Layer
        await GenerateApplicationLayer(rootDir, projectName, model);

        // 3. Generate Infrastructure Layer
        await GenerateInfrastructureLayer(rootDir, projectName, model, dbProvider);

        // 4. Generate Api Layer
        await GenerateApiLayer(rootDir, projectName, model, apiType);

        // 5. Generate Architecture Tests
        await GenerateArchitectureTests(rootDir, projectName, model);

        // 6. Generate Dockerfile (Root level)
        await GenerateFile("Api/Dockerfile.liquid", Path.Combine(rootDir, "Dockerfile"), model);

        // 7. Generate README.md
        await GenerateFile("README.md.liquid", Path.Combine(rootDir, "README.md"), model);

        // 8. Generate .editorconfig
        await GenerateFile("editorconfig.liquid", Path.Combine(rootDir, ".editorconfig"), model);

        // 9. Create Solution and add projects
        var layers = new[] { "Domain", "Application", "Infrastructure", "Api", "ArchitectureTests" };
        await CreateSolutionAsync(rootDir, projectName, layers);
    }

    private async Task GenerateArchitectureTests(string rootDir, string projectName, object model)
    {
        var projectPath = Path.Combine(rootDir, $"{projectName}.ArchitectureTests");
        Directory.CreateDirectory(projectPath);

        await GenerateFile("ArchitectureTests.csproj.liquid", Path.Combine(projectPath, $"{projectName}.ArchitectureTests.csproj"), model);
        await GenerateFile("ArchitectureTests.cs.liquid", Path.Combine(projectPath, "ArchitectureTests.cs"), model);
    }

    public async Task GenerateResourceAsync(string projectName, string solutionRoot, string resourceName, string apiType)
    {
        var model = new { ProjectName = projectName, ResourceName = resourceName, ApiType = apiType };

        // 1. Domain Entity
        var domainPath = Path.Combine(solutionRoot, $"{projectName}.Domain", "Entities");
        Directory.CreateDirectory(domainPath);
        await GenerateFile("Resource/Entity.cs.liquid", Path.Combine(domainPath, $"{resourceName}.cs"), model);

        // 2. Application Handlers (Create)
        var appPath = Path.Combine(solutionRoot, $"{projectName}.Application", $"{resourceName}s", "Create");
        Directory.CreateDirectory(appPath);
        await GenerateFile("Resource/CreateCommand.cs.liquid", Path.Combine(appPath, $"Create{resourceName}Command.cs"), model);
        await GenerateFile("Resource/CreateCommandHandler.cs.liquid", Path.Combine(appPath, $"Create{resourceName}CommandHandler.cs"), model);

        // 3. Api Layer (Controller or Endpoints)
        if (apiType == "controllers")
        {
            var apiPath = Path.Combine(solutionRoot, $"{projectName}.Api", "Controllers");
            Directory.CreateDirectory(apiPath);
            await GenerateFile("Resource/Controller.cs.liquid", Path.Combine(apiPath, $"{resourceName}sController.cs"), model);
        }
        else
        {
            var apiPath = Path.Combine(solutionRoot, $"{projectName}.Api", "Endpoints");
            Directory.CreateDirectory(apiPath);
            await GenerateFile("Resource/Endpoints.cs.liquid", Path.Combine(apiPath, $"{resourceName}Endpoints.cs"), model);
        }
    }

    private async Task GenerateApiLayer(string rootDir, string projectName, object model, string apiType)
    {
        var projectPath = Path.Combine(rootDir, $"{projectName}.Api");
        Directory.CreateDirectory(projectPath);

        var apiModel = new { ProjectName = projectName, Layer = "Api", ApiType = apiType };
        await GenerateFile("Project.csproj.liquid", Path.Combine(projectPath, $"{projectName}.Api.csproj"), apiModel);

        // Program.cs & AppSettings
        await GenerateFile("Api/Program.cs.liquid", Path.Combine(projectPath, "Program.cs"), apiModel);
        await GenerateFile("Api/appsettings.json.liquid", Path.Combine(projectPath, "appsettings.json"), apiModel);

        // Middleware
        var middlewarePath = Path.Combine(projectPath, "Middleware");
        Directory.CreateDirectory(middlewarePath);
        await GenerateFile("Api/GlobalExceptionHandler.cs.liquid", Path.Combine(middlewarePath, "GlobalExceptionHandler.cs"), apiModel);

        if (apiType == "controllers")
        {
            // Abstractions
            var abstractionsPath = Path.Combine(projectPath, "Abstractions");
            Directory.CreateDirectory(abstractionsPath);
            await GenerateFile("Api/ApiController.cs.liquid", Path.Combine(abstractionsPath, "ApiController.cs"), apiModel);
        }
    }

    private async Task GenerateInfrastructureLayer(string rootDir, string projectName, object model, string dbProvider)
    {
        var projectPath = Path.Combine(rootDir, $"{projectName}.Infrastructure");
        Directory.CreateDirectory(projectPath);

        var infraModel = new { ProjectName = projectName, Layer = "Infrastructure", DbProvider = dbProvider };
        await GenerateFile("Project.csproj.liquid", Path.Combine(projectPath, $"{projectName}.Infrastructure.csproj"), infraModel);

        // Persistence
        var persistencePath = Path.Combine(projectPath, "Persistence");
        Directory.CreateDirectory(persistencePath);
        await GenerateFile("Infrastructure/ApplicationDbContext.cs.liquid", Path.Combine(persistencePath, "ApplicationDbContext.cs"), infraModel);

        // Repositories
        var repositoriesPath = Path.Combine(projectPath, "Repositories");
        Directory.CreateDirectory(repositoriesPath);
        await GenerateFile("Infrastructure/Repository.cs.liquid", Path.Combine(repositoriesPath, "Repository.cs"), infraModel);
        await GenerateFile("Infrastructure/UnitOfWork.cs.liquid", Path.Combine(repositoriesPath, "UnitOfWork.cs"), infraModel);

        // Dependency Injection
        await GenerateFile("Infrastructure/DependencyInjection.cs.liquid", Path.Combine(projectPath, "DependencyInjection.cs"), infraModel);
    }

    private async Task GenerateApplicationLayer(string rootDir, string projectName, object model)
    {
        var projectPath = Path.Combine(rootDir, $"{projectName}.Application");
        Directory.CreateDirectory(projectPath);

        // .csproj (passed layer name to template)
        var appModel = new { ProjectName = projectName, Layer = "Application" };
        await GenerateFile("Project.csproj.liquid", Path.Combine(projectPath, $"{projectName}.Application.csproj"), appModel);

        // Abstractions/Messaging
        var messagingPath = Path.Combine(projectPath, "Abstractions", "Messaging");
        Directory.CreateDirectory(messagingPath);
        await GenerateFile("Application/ICommand.cs.liquid", Path.Combine(messagingPath, "ICommand.cs"), appModel);
        await GenerateFile("Application/ICommandHandler.cs.liquid", Path.Combine(messagingPath, "ICommandHandler.cs"), appModel);
        await GenerateFile("Application/IQuery.cs.liquid", Path.Combine(messagingPath, "IQuery.cs"), appModel);
        await GenerateFile("Application/IQueryHandler.cs.liquid", Path.Combine(messagingPath, "IQueryHandler.cs"), appModel);

        // Behaviors
        var behaviorsPath = Path.Combine(projectPath, "Behaviors");
        Directory.CreateDirectory(behaviorsPath);
        await GenerateFile("Application/ValidationBehavior.cs.liquid", Path.Combine(behaviorsPath, "ValidationBehavior.cs"), appModel);

        // Dependency Injection
        await GenerateFile("Application/DependencyInjection.cs.liquid", Path.Combine(projectPath, "DependencyInjection.cs"), appModel);
    }

    private async Task GenerateDomainLayer(string rootDir, string projectName, object model)
    {
        var projectPath = Path.Combine(rootDir, $"{projectName}.Domain");
        Directory.CreateDirectory(projectPath);

        // .csproj
        var domainModel = new { ProjectName = projectName, Layer = "Domain" };
        await GenerateFile("Project.csproj.liquid", Path.Combine(projectPath, $"{projectName}.Domain.csproj"), domainModel);

        // Primitives
        var primitivesPath = Path.Combine(projectPath, "Primitives");
        Directory.CreateDirectory(primitivesPath);
        await GenerateFile("Domain/Entity.cs.liquid", Path.Combine(primitivesPath, "Entity.cs"), domainModel);
        await GenerateFile("Domain/ValueObject.cs.liquid", Path.Combine(primitivesPath, "ValueObject.cs"), domainModel);

        // Abstractions
        var domainAbsPath = Path.Combine(projectPath, "Abstractions");
        Directory.CreateDirectory(domainAbsPath);
        await GenerateFile("Domain/IRepository.cs.liquid", Path.Combine(domainAbsPath, "IRepository.cs"), domainModel);
        await GenerateFile("Domain/IUnitOfWork.cs.liquid", Path.Combine(domainAbsPath, "IUnitOfWork.cs"), domainModel);

        // Shared
        var sharedPath = Path.Combine(projectPath, "Shared");
        Directory.CreateDirectory(sharedPath);
        await GenerateFile("Domain/Result.cs.liquid", Path.Combine(sharedPath, "Result.cs"), domainModel);
        await GenerateFile("Domain/Error.cs.liquid", Path.Combine(sharedPath, "Error.cs"), domainModel);
        await GenerateFile("Domain/ValidationResult.cs.liquid", Path.Combine(sharedPath, "ValidationResult.cs"), domainModel);
        await GenerateFile("Domain/PagedList.cs.liquid", Path.Combine(sharedPath, "PagedList.cs"), domainModel);
    }

    private async Task GenerateLayerStub(string rootDir, string projectName, string layerName, object model)
    {
        var projectPath = Path.Combine(rootDir, $"{projectName}.{layerName}");
        Directory.CreateDirectory(projectPath);
        var stubModel = new { ProjectName = projectName, Layer = layerName };
        await GenerateFile("Project.csproj.liquid", Path.Combine(projectPath, $"{projectName}.{layerName}.csproj"), stubModel);
    }

    private async Task GenerateFile(string templateName, string outputPath, object model)
    {
        var templateFile = Path.Combine(_templatePath, templateName);
        if (!File.Exists(templateFile)) return;

        var content = await File.ReadAllTextAsync(templateFile);
        var rendered = _engine.Render(content, model);
        await File.WriteAllTextAsync(outputPath, rendered);
    }

    private async Task CreateSolutionAsync(string rootDir, string projectName, string[] layers)
    {
        var slnName = $"{projectName}.sln";
        
        // 1. Create the solution file
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new sln -n {projectName}",
                WorkingDirectory = rootDir,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        // 2. Add projects to the solution
        foreach (var layer in layers)
        {
            var projectPath = Path.Combine($"{projectName}.{layer}", $"{projectName}.{layer}.csproj");
            
            var addProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"sln {slnName} add {projectPath}",
                    WorkingDirectory = rootDir,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            addProcess.Start();
            await addProcess.WaitForExitAsync();
        }
    }
}
