namespace App.Features.BackgroundWorker.GetAdminsFromMgm;

public interface IGetAdminsFromMgm
{
    Task<IReadOnlyList<MgmAdmin>> Execute(CancellationToken ct = default);
}

public sealed record MgmAdmin(string Email, string Name, string LdapUsername);