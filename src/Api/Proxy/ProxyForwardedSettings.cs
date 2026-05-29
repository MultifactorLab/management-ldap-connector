using System.ComponentModel.DataAnnotations;

namespace Api.Proxy;

internal sealed class ProxyForwardedSettings
{
    /// <summary>
    /// Например, "X-Real-IP2"
    /// </summary>
    public string? ForwardedForHeaderName { get; init; } = string.Empty;

    public required List<KnownNetwork> KnownNetworks { get; init; } = [];
}

internal sealed class KnownNetwork
{
    public required string Prefix { get; init; }
    public required int PrefixLength { get; init; }
}