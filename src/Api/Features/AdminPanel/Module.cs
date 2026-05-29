using Api.Features.AdminPanel.AdminAccount.ChangeAdminPassword;
using Api.Features.AdminPanel.AdminAccount.GetAdminAccount;
using Api.Features.AdminPanel.AdminAccount.SaveAdminAccount;
using Api.Features.AdminPanel.LdapSettings.GetLdapSettings;
using Api.Features.AdminPanel.LdapSettings.SaveLdapSettings;
using Api.Features.AdminPanel.LdapSettings.TestLdapConnection;
using Api.Features.AdminPanel.Login;
using Api.Features.AdminPanel.SyncSettings.GetSyncSettings;
using Api.Features.AdminPanel.SyncSettings.SaveSyncSettings;
using Api.Features.AdminPanel.SyncStatus.GetSyncStatus;
using Api.Features.AdminPanel.SyncStatus.SaveSyncStatus;

namespace Api.Features.AdminPanel;

internal static class Module
{
    public static void AddAdminPanelFeatures(this WebApplicationBuilder builder)
    {
        builder.AddGetAdminAccountFeature();
        builder.AddSaveAdminAccountFeature();
        builder.AddChangeAdminPasswordFeature();
        builder.AddGetLdapSettingsFeature();
        builder.AddSaveLdapSettingsFeature();
        builder.AddTestLdapConnectionFeature();
        builder.AddGetSyncSettingsFeature();
        builder.AddSaveSyncSettingsFeature();
        builder.AddGetSyncStatusFeature();
        builder.AddSaveSyncStatusFeature();
        builder.AddLoginFeature();
    }
}