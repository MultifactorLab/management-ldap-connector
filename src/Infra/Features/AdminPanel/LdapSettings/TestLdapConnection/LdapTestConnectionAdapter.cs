using System.DirectoryServices.Protocols;
using System.Net;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection;
using App.Features.AdminPanel.LdapSettings.TestLdapConnection.Models;
using Infra.Integrations.LdapConnectionFactory;
using Infra.SharedAdapters.LdapExecutor;

namespace Infra.Features.AdminPanel.LdapSettings.TestLdapConnection;

internal sealed class LdapTestConnectionAdapter : ITestLdapConnection
{
    private readonly ILdapConnectionFactory _connectionFactory;
    private readonly ILdapExecutor _executor;
 
    public LdapTestConnectionAdapter(
        ILdapConnectionFactory connectionFactory,
        ILdapExecutor executor)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
    }
 
    public TestLdapConnectionResult Execute(TestLdapConnectionModel testConnectionModel)
    {
        try
        {
            using var connection = _connectionFactory.Create(
                new LdapConnectionFactoryModel(
                    testConnectionModel.ServerUrl,
                    testConnectionModel.Port,
                    testConnectionModel.UseSsl));
 
            _executor.Bind(connection, new NetworkCredential(
                testConnectionModel.ServiceAccountDn,
                testConnectionModel.BindPassword));
 
            return TestLdapConnectionResult.Success();
        }
        catch (LdapException ex) when (ex.ErrorCode == LdapErrorCodes.InvalidCredentials)
        {
            return TestLdapConnectionResult.Failure(
                TestLdapConnectionError.InvalidCredentials,
                ex.Message);
        }
        catch (LdapException ex)
        {
            return TestLdapConnectionResult.Failure(
                TestLdapConnectionError.ConnectionFailed,
                ex.Message);
        }
        catch (Exception ex)
        {
            return TestLdapConnectionResult.Failure(
                TestLdapConnectionError.ConnectionFailed,
                ex.Message);
        }
    }
}