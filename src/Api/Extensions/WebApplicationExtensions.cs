using Api.Framework.Authentication;
using Api.Framework.EndpointDiscovery;
using Microsoft.AspNetCore.Authentication;

namespace Api.Extensions;

internal static class WebApplicationExtensions
{
    /// <summary>
    /// Настраивает мэппинг http-роутов. 
    /// </summary>
    /// <param name="app"></param>
    public static void MapFeatures(this WebApplication app)
    {
        EndpointMapper.MapEndpoints(app);
        
        app.MapGet("/", () => Results.Redirect("/settings/ldap"));
    }

    public static void ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            })
            .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", _ => { });
    }
}