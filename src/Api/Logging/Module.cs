using Destructurama;
using Serilog;

namespace Api.Logging;

internal static class Module
{
    public static void AddLogging(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile(
                $"serilog.{builder.Environment.EnvironmentName}.json",
                optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables();
        
        builder.Services.AddHttpContextAccessor();
        Serilog.Debugging.SelfLog.Enable(Console.Error);
        builder.Host.UseSerilog((ctx, prov, config) =>
        {
            config.ReadFrom.Configuration(ctx.Configuration)
                .Destructure.UsingAttributes(x => x.IgnoreNullProperties = true);
        });
    }
}