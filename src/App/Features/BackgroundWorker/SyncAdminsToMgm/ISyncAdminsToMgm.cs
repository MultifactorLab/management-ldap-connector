namespace App.Features.BackgroundWorker.SyncAdminsToMgm;

public interface ISyncAdminsToMgm
{
    Task Execute(IReadOnlyList<AdminDefinition> admins, CancellationToken ct = default);
}

public sealed record AdminDefinition(string Email, string Name, string LdapUsername);