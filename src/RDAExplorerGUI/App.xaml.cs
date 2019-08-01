using RDAExplorer;
using RDAExplorerGUI.Properties;
using System.Windows;

namespace RDAExplorerGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UISettings.EnableConsole = false;

            // When the version number has changed the application would load the default settings again.
            // We detect this by checking SettingsUpgradeNeeded (true by default), and then calling Upgrade() if necessary.
            // Note that if we didn't check SettingsUpgradeNeeded we would always load the settings from the previous version.
            if (Settings.Default.SettingsUpgradeNeeded) {
                Settings.Default.Upgrade();
                Settings.Default.SettingsUpgradeNeeded = false;
                Settings.Default.Save();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
