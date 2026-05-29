using System.DirectoryServices.Protocols;

namespace Infra.Integrations.LdapConnectionFactory;

internal interface ILdapConnectionFactory
{
    LdapConnection Create(LdapConnectionFactoryModel model);
}

internal sealed class LdapConnectionFactory : ILdapConnectionFactory
{
    private const int ProtocolVersion = 3;
    private const int TimeoutInSec = 10;

    public LdapConnection Create(LdapConnectionFactoryModel model)
    {
        var identifier = new LdapDirectoryIdentifier(model.ServerUrl, model.Port);

        var connection = new LdapConnection(identifier)
        {
            AuthType = AuthType.Basic,
            AutoBind = false
        };
        
        connection.SessionOptions.ProtocolVersion = ProtocolVersion;
        connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

        if (model.UseSsl)
        {
            connection.SessionOptions.SecureSocketLayer = true;
        }

        return connection;
    }
}

public record LdapConnectionFactoryModel(string ServerUrl, int Port, bool UseSsl);