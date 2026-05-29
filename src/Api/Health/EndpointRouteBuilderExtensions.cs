using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Health;

internal static class EndpointRouteBuilderExtensions
{
    public static void UseHealthCheckV2(this IEndpointRouteBuilder app)
    {
        var env = app.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (env.EnvironmentName == "test")
        {
            return;
        }
        
        app.MapHealthChecks("/v2/health/startup", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("startup"),
            ResponseWriter = MinimalWriter
        });
        
        app.MapHealthChecks("/v2/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = MinimalWriter
        });
        
        app.MapHealthChecks("/v2/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            ResponseWriter = MinimalWriter
        });
    }
    
    private static Task MinimalWriter(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "text/plain";
        ctx.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 : 500;
        return ctx.Response.WriteAsync(report.Status.ToString());
    }
}