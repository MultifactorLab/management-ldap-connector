using System.Text;

namespace Api.Framework.Authentication;

internal record BasicAuthCredential
{
    public string Username { get; }
    public string Password { get; }

    private BasicAuthCredential(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public static BasicAuthCredential? GetFromHeaders(IHeaderDictionary headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        
        var authHeader = headers.Authorization;
        if (authHeader.Count == 0)
        {
            return null;
        }

        var value = (string?)authHeader;
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        
        if (!value.StartsWith("Basic ", StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        var bytes = Convert.FromBase64String(value[6..]);
        var asciiString = Encoding.ASCII.GetString(bytes);
        var segments = asciiString.Split(':');
        if (segments.Length != 2)
        {
            return null;
        }
        
        if (segments.Any(string.IsNullOrWhiteSpace))
        {
            return null;
        }

        return new BasicAuthCredential(segments[0], segments[1]);
    }
}
