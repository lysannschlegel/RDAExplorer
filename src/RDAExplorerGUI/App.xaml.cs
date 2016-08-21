using RDAExplorer;
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
        }
    }
}
