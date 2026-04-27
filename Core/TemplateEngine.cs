using Fluid;

namespace NetHexaGen.Core;

public class TemplateEngine
{
    private readonly FluidParser _parser = new FluidParser();

    public string Render(string templateContent, object model)
    {
        if (_parser.TryParse(templateContent, out var template, out var error))
        {
            var context = new TemplateContext(model);
            context.Options.MemberAccessStrategy.Register(model.GetType());
            return template.Render(context);
        }

        throw new Exception($"Error parsing template: {error}");
    }
}
