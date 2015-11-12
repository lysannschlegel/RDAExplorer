using RDAExplorer;
using System.Windows;

namespace RDAExplorerGUI.Misc
{
    public partial class RDASkippedDataSectionTreeViewItem
    {
        public RDASkippedDataSection Section;

        public RDASkippedDataSectionTreeViewItem()
        {
            InitializeComponent();
            SelectOnRightClick = true;
        }

        private void context_Delete_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentMainWindow.CurrentReader.SkippedDataSections.Remove(Section);
            MainWindow.CurrentMainWindow.RebuildTreeView();
        }
    }
}
