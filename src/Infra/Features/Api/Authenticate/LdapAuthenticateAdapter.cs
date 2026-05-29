using System.DirectoryServices.Protocols;
using System.Net;
using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using App.Features.Api.Authenticate;
using Infra.Features.Api.Authenticate.Strategies;
using Infra.Integrations.LdapConnectionFactory;
using Infra.SharedAdapters.LdapExecutor;

namespace Infra.Features.Api.Authenticate;

internal sealed class LdapAuthenticateAdapter : IAuthenticateLdapUser
{
    private readonly ILdapConnectionFactory _connectionFactory;
    private readonly ILdapExecutor _executor;
    private readonly ILdapAuthenticationStrategy _strategy;
 
    public LdapAuthenticateAdapter(
        ILdapConnectionFactory connectionFactory,
        ILdapExecutor executor,
        ILdapAuthenticationStrategy strategy)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
        _strategy = strategy;
    }
 
    public AuthenticateLdapUserResult Execute(LdapSettingsModel settings, UserCredential credential)
    {
        try
        {
            using var connection = _connectionFactory.Create(
                new LdapConnectionFactoryModel(
                    settings.ServerUrl,
                    settings.Port,
                    settings.UseSsl));
 
            _executor.Bind(connection, new NetworkCredential(
                settings.ServiceAccountDn,
                settings.BindPassword));
 
            return _strategy.Authenticate(connection, settings, credential.Username, credential.Password, _executor);
        }
        catch (LdapException ex) when (ex.ErrorCode == LdapErrorCodes.InvalidCredentials)
        {
            return AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.InvalidCredentials);
        }
        catch (LdapException)
        {
            return AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.ConnectionFailed);
        }
        catch (Exception)
        {
            return AuthenticateLdapUserResult.Failure(AuthenticateLdapUserError.ConnectionFailed);
        }
    }
}