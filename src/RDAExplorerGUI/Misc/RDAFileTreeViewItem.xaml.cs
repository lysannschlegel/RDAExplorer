using AnnoModificationManager4.Misc;
using AnnoModificationManager4.UserInterface.Misc;
using RDAExplorer;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace RDAExplorerGUI.Misc
{
    public partial class RDAFileTreeViewItem
    {
        public RDAFile File;

        public RDAFileTreeViewItem()
        {
            InitializeComponent();
            SelectOnRightClick = true;
        }

        private void context_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!System.IO.File.Exists(DirectoryExtension.GetTempWorkingDirectory() + "\\" + File.FileName))
                    File.ExtractToRoot(DirectoryExtension.GetTempWorkingDirectory());
                Process.Start(DirectoryExtension.GetTempWorkingDirectory() + "\\" + File.FileName);
                MainWindow.CurrentMainWindow.FileWatcher.Changed += new FileSystemEventHandler(FileWatcher_Changed);
                MainWindow.CurrentMainWindow.FileWatcher.Deleted += new FileSystemEventHandler(FileWatcher_Deleted);
            }
            catch (Exception ex)
            {
                int num = (int)MessageWindow.Show(ex.Message);
            }
        }

        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            MainWindow.CurrentMainWindow.FileWatcher.Changed -= new FileSystemEventHandler(FileWatcher_Changed);
            MainWindow.CurrentMainWindow.FileWatcher.Deleted -= new FileSystemEventHandler(FileWatcher_Deleted);
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!(e.FullPath.Replace(DirectoryExtension.GetTempWorkingDirectory(), "").Trim('\\') == File.FileName.Replace("/", "\\")) || MainWindow.CurrentMainWindow.FileWatcher_ToUpdate.Contains(File))
                return;
            MainWindow.CurrentMainWindow.FileWatcher_ToUpdate.Add(File);
        }

        private void context_Extract_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = Path.GetFileName(File.FileName);
            bool? nullable = saveFileDialog.ShowDialog();
            if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                return;
            try
            {
                File.Extract(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                int num = (int)MessageWindow.Show(ex.Message);
            }
        }

        private void context_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is RDAFolderTreeViewItem)
            {
                RDAFolderTreeViewItem folderTreeViewItem = Parent as RDAFolderTreeViewItem;
                folderTreeViewItem.Folder.Files.Remove(File);
                folderTreeViewItem.UpdateSubItems();
            }
            else
            {
                MainWindow.CurrentMainWindow.CurrentReader.rdaFolder.Files.Remove(File);
                MainWindow.CurrentMainWindow.RebuildTreeView();
            }
        }

        private void ModifiedTreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            context_Open_Click(null, null);
        }
    }
}
