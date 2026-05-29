using App.Features.AdminPanel.LdapSettings.Models;
using App.Features.AdminPanel.SyncStatus.SaveSyncStatus;
using App.Features.AdminPanel.SyncStatus.SaveSyncStatus.Models;
using App.Features.BackgroundWorker.GetAdminsFromMgm;
using App.Features.BackgroundWorker.GetUsersFromActiveDirectory;
using App.SharedPorts;
using App.SharedPorts.Encryption.DecryptValue;
using Microsoft.Extensions.Logging;

namespace App.Features.BackgroundWorker.SyncAdminsToMgm;

public interface ISyncUsersUseCase
{
    Task<SyncUsersResult> Execute(CancellationToken ct = default);
}

internal sealed class SyncAdminsToMgmUseCase : ISyncUsersUseCase
{
    private readonly IGetSyncSettings _getSyncSettings;
    private readonly IGetLdapSettings _getLdapSettings;
    private readonly IDecryptValue _decryptValue;
    private readonly IGetUsersFromActiveDirectory _getUsersFromActiveDirectory;
    private readonly IGetAdminsFromMgm _getAdminsFromMgm;
    private readonly ISyncAdminsToMgm _syncAdminsToMgm;
    private readonly ISaveSyncStatus _saveSyncStatus;
    private readonly ILogger<SyncAdminsToMgmUseCase> _logger;

    public SyncAdminsToMgmUseCase(
        IGetSyncSettings getSyncSettings,
        IGetLdapSettings getLdapSettings,
        IDecryptValue decryptValue,
        IGetUsersFromActiveDirectory getUsersFromActiveDirectory,
        IGetAdminsFromMgm getAdminsFromMgm,
        ISyncAdminsToMgm syncAdminsToMgm,
        ISaveSyncStatus saveSyncStatus,
        ILogger<SyncAdminsToMgmUseCase> logger)
    {
        _getSyncSettings = getSyncSettings;
        _getLdapSettings = getLdapSettings;
        _decryptValue = decryptValue;
        _getUsersFromActiveDirectory = getUsersFromActiveDirectory;
        _getAdminsFromMgm = getAdminsFromMgm;
        _syncAdminsToMgm = syncAdminsToMgm;
        _saveSyncStatus = saveSyncStatus;
        _logger = logger;
    }

    public async Task<SyncUsersResult> Execute(CancellationToken ct = default)
    {
        // Загружаем настройки
        var syncSettings = _getSyncSettings.Execute();
        if (syncSettings is null)
        {
            const string error = "Sync settings not configured.";
            _logger.LogWarning(error);
            SaveStatus(SyncUsersResult.Failure(error), ct);
            return SyncUsersResult.Failure(error);
        }

        var ldapSettings = _getLdapSettings.Execute();
        if (ldapSettings is null)
        {
            const string error = "LDAP settings not configured.";
            _logger.LogWarning(error);
            SaveStatus(SyncUsersResult.Failure(error), ct);
            return SyncUsersResult.Failure(error);
        }

        var decryptResult = _decryptValue.Execute(new DecryptValueRequest(ldapSettings.BindPassword));

        var ldapSettingsWithPlainPassword = ldapSettings with
        {
            BindPassword = decryptResult.PlainText
        };

        var ldapSettingsModel = new LdapSettingsModel
        {
            BaseDn = ldapSettingsWithPlainPassword.BaseDn,
            ServerUrl = ldapSettingsWithPlainPassword.ServerUrl,
            BindPassword = ldapSettingsWithPlainPassword.BindPassword,
            DisplayNameAttribute = ldapSettingsWithPlainPassword.DisplayNameAttribute,
            EmailAttribute = ldapSettingsWithPlainPassword.EmailAttribute,
            UsernameAttribute = ldapSettingsWithPlainPassword.UsernameAttribute,
            ServiceAccountDn = ldapSettingsWithPlainPassword.ServiceAccountDn,
            Port = ldapSettingsWithPlainPassword.Port,
            UseSsl = ldapSettingsWithPlainPassword.UseSsl,
            SyncGroupDn = ldapSettingsWithPlainPassword.SyncGroupDn,
        };

        try
        {
            // Получаем пользователей из LDAP
            var ldapUsers = _getUsersFromActiveDirectory.Execute(ldapSettingsModel);

            if (ldapUsers.Count == 0)
            {
                _logger.LogInformation("No users found in LDAP. Skipping sync.");
                var result = SyncUsersResult.NoChanges();
                SaveStatus(result, ct);
                return result;
            }

            // Маппим LDAP пользователей на AdminDefinition
            var ldapAdmins = new List<AdminDefinition>();

            foreach (var user in ldapUsers)
            {
                if (string.IsNullOrWhiteSpace(user.Email) ||
                    string.IsNullOrWhiteSpace(user.DisplayName) ||
                    string.IsNullOrWhiteSpace(user.Username))
                {
                    _logger.LogWarning(
                        "Skipping LDAP user with incomplete attributes. " +
                        "Email: '{Email:l}', DisplayName: '{DisplayName:l}', Username: '{Username:l}'.",
                        user.Email, user.DisplayName, user.Username);
                    continue;
                }

                ldapAdmins.Add(new AdminDefinition(
                    Email: user.Email,
                    Name: user.DisplayName,
                    LdapUsername: user.Username));
            }

            if (ldapAdmins.Count < ldapUsers.Count)
            {
                _logger.LogWarning(
                    "Skipped {SkippedCount} of {TotalCount} LDAP users due to incomplete attributes.",
                    ldapUsers.Count - ldapAdmins.Count,
                    ldapUsers.Count);
            }

            // Получаем существующих пользователей (админов) из MGM
            var existingAdmins = await _getAdminsFromMgm.Execute(ct);
            var existingByEmail = existingAdmins
                .ToDictionary(a => a.Email, StringComparer.OrdinalIgnoreCase);

            // Находим новых — тех кого нет в MGM
            var toSync = ldapAdmins
                .Where(a =>
                {
                    // Новый — нет в MGM
                    if (!existingByEmail.TryGetValue(a.Email, out var existing))
                    {
                        return true;
                    }
                    
                    // Изменённый — есть в MGM, но отличается Name или LdapUsername
                    return !string.Equals(a.Name, existing.Name, StringComparison.OrdinalIgnoreCase) ||
                           !string.Equals(a.LdapUsername, existing.LdapUsername, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
            
            if (toSync.Count == 0)
            {
                _logger.LogInformation("No new admins to sync.");
                var result = SyncUsersResult.NoChanges();
                SaveStatus(result, ct);
                return result;
            }

            // Отправляем новых в MGM
            await _syncAdminsToMgm.Execute(toSync, ct);

            _logger.LogInformation("Synced {Count} admins to MGM.", toSync.Count);

            var successResult = SyncUsersResult.Success(toSync.Count);
            SaveStatus(successResult, ct);
            return successResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed: {Message}.", ex.Message);
            var result = SyncUsersResult.Failure(ex.Message);
            SaveStatus(result, ct);
            return result;
        }
    }

    private void SaveStatus(SyncUsersResult result, CancellationToken ct)
    {
        _saveSyncStatus.Execute(new SaveSyncStatusModel
        {
            LastRunAt = DateTime.Now,
            SyncedCount = result.SyncedCount,
            Success = result.IsSuccess,
            ErrorMessage = result.ErrorMessage
        });
    }
}

public sealed record SyncUsersResult
{
    public bool IsSuccess { get; init; }
    public int SyncedCount { get; init; }
    public string? ErrorMessage { get; init; }

    public static SyncUsersResult Success(int syncedCount) => new()
    {
        IsSuccess = true,
        SyncedCount = syncedCount
    };

    public static SyncUsersResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };

    public static SyncUsersResult NoChanges() => new()
    {
        IsSuccess = true,
        SyncedCount = 0
    };
}