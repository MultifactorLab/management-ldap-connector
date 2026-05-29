using System.DirectoryServices.Protocols;
using System.Net;

namespace Infra.SharedAdapters.LdapExecutor;

internal interface ILdapExecutor
{
    void Bind(LdapConnection connection, NetworkCredential credential);
    DirectoryResponse SendRequest(LdapConnection connection, DirectoryRequest request);
}
 
internal sealed class LdapExecutor : ILdapExecutor
{
    public void Bind(LdapConnection connection, NetworkCredential credential)
    {
        connection.Bind(credential);
    }
 
    public DirectoryResponse SendRequest(LdapConnection connection, DirectoryRequest request)
    {
        return connection.SendRequest(request);
    }
}