using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Api.Proxy;

internal static class Module
{
    public static void ConfigureNginxProxy(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            
            var proxyForwardedSettings = builder.Configuration
                .GetSection("ProxyForwardedSettings")
                .Get<ProxyForwardedSettings>();

            if (proxyForwardedSettings == null)
            {
                return;
            }
            
            if (!string.IsNullOrWhiteSpace(proxyForwardedSettings.ForwardedForHeaderName))
            {
                options.ForwardedForHeaderName = proxyForwardedSettings.ForwardedForHeaderName;
            }

            foreach (var knownNetwork in proxyForwardedSettings.KnownNetworks)
            {
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse(knownNetwork.Prefix), knownNetwork.PrefixLength));
            }
        });
    }
}