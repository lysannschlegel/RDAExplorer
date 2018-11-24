using AnnoModificationManager4.Controls;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.UserInterface.Misc;
using RDAExplorer;
using RDAExplorerGUI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace RDAExplorerGUI.Misc
{
    public partial class RDAFolderTreeViewItem
    {
        public RDAFolder Folder;
        private bool AlreadyExpanded;

        public RDAFolderTreeViewItem()
        {
            InitializeComponent();
            SelectOnRightClick = true;
            Expanded += new RoutedEventHandler(RDAFolderTreeViewItem_Expanded);
            Items.Add(new ModifiedTreeViewItem());
        }

        public void UpdateSubItems()
        {
            Items.Clear();
            foreach (RDAFolder rdaFolder in Enumerable.OrderBy(Folder.Folders, f => f.Name))
            {
                RDAFolderTreeViewItem folderTreeViewItem = new RDAFolderTreeViewItem();
                folderTreeViewItem.Folder = rdaFolder;
                folderTreeViewItem.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/folder.png", rdaFolder.Name);
                Items.Add(folderTreeViewItem);
                if ((TreeViewExtension.GetTreeView(this) as MultiSelectTreeView).SelectedItems.Contains(this))
                {
                    (TreeViewExtension.GetTreeView(this) as MultiSelectTreeView).SelectItem(folderTreeViewItem);
                    (TreeViewExtension.GetTreeView(this) as MultiSelectTreeView).UpdateSelectedItems();
                }
            }
            foreach (RDAFile file in Enumerable.OrderBy(Folder.Files, f => f.FileName))
            {
                RDAFileTreeViewItem fileTreeViewItem = RDAFileExtension.ToTreeViewItem(file);
                fileTreeViewItem.SelectOnRightClick = true;
                Items.Add(fileTreeViewItem);
                if ((TreeViewExtension.GetTreeView(this) as MultiSelectTreeView).SelectedItems.Contains(this))
                {
                    (TreeViewExtension.GetTreeView(this) as MultiSelectTreeView).SelectItem(fileTreeViewItem);
                    (TreeViewExtension.GetTreeView(this) as MultiSelectTreeView).UpdateSelectedItems();
                }
            }
            AlreadyExpanded = true;
            IsExpanded = true;
        }

        private void RDAFolderTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (AlreadyExpanded)
                return;
            UpdateSubItems();
        }

        public RDAFolderTreeViewItem SearchFolder(string text)
        {
            RDAFolderTreeViewItem_Expanded(null, null);
            if (Folder.Name.Contains(text))
                return this;
            foreach (RDAFolderTreeViewItem folderTreeViewItem1 in Enumerable.OfType<RDAFolderTreeViewItem>(Items))
            {
                RDAFolderTreeViewItem folderTreeViewItem2 = folderTreeViewItem1.SearchFolder(text);
                if (folderTreeViewItem2 != null)
                    return folderTreeViewItem2;
            }
            IsExpanded = false;
            return null;
        }

        public RDAFileTreeViewItem SearchFile(string text)
        {
            RDAFolderTreeViewItem_Expanded(null, null);
            foreach (RDAFileTreeViewItem fileTreeViewItem in Enumerable.OfType<RDAFileTreeViewItem>(Items))
            {
                if (Path.GetFileName(fileTreeViewItem.File.FileName).Contains(text))
                    return fileTreeViewItem;
            }
            foreach (RDAFolderTreeViewItem folderTreeViewItem in Enumerable.OfType<RDAFolderTreeViewItem>(Items))
            {
                RDAFileTreeViewItem fileTreeViewItem = folderTreeViewItem.SearchFile(text);
                if (fileTreeViewItem != null)
                    return fileTreeViewItem;
            }
            IsExpanded = false;
            return null;
        }

        private void context_Extract_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            MainWindow.CurrentMainWindow.progressBar_Status.Visibility = Visibility.Visible;
            wrk.ProgressChanged += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, (() =>
            {
                MainWindow.CurrentMainWindow.label_Status.Text = RDAExplorer.RDAFileExtension.ExtractAll_LastMessage;
                MainWindow.CurrentMainWindow.progressBar_Status.Value = e2.ProgressPercentage;
            }));
            wrk.RunWorkerCompleted += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, (() =>
            {
                MainWindow.CurrentMainWindow.label_Status.Text = MainWindow.CurrentMainWindow.CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                MainWindow.CurrentMainWindow.progressBar_Status.Visibility = Visibility.Collapsed;
            }));
            RDAExplorer.RDAFileExtension.ExtractAll(Folder.GetAllFiles(), folderBrowserDialog.SelectedPath, wrk);
            wrk.RunWorkerAsync();
        }

        private void context_AddFiles_Click(object sender, RoutedEventArgs e)
        {
            AnnoModificationManager4.Misc.OpenFileDialog openFileDialog = new AnnoModificationManager4.Misc.OpenFileDialog();
            openFileDialog.Filter = "All files|*.*";
            openFileDialog.Multiselect = true;
            bool? nullable = openFileDialog.ShowDialog();
            if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                return;
            foreach (string file in openFileDialog.FileNames)
            {
                string generatedRDAFileName = RDAFile.FileNameToRDAFileName(file, Folder.FullPath);
                RDAFile rdafile = Folder.Files.Find(f => f.FileName == generatedRDAFileName);
                if (rdafile == null)
                {
                    RDAFile rdaFile = RDAFile.Create(Folder.Version, file, Folder.FullPath);
                    if (rdaFile != null)
                        Folder.Files.Add(rdaFile);
                }
                else
                    RDAFileExtension.SetFile(rdafile, file, true);
            }
            UpdateSubItems();
        }

        private void context_AddFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            List<RDAFile> files = new List<RDAFile>();
            foreach (string str in Directory.GetFiles(folderBrowserDialog.SelectedPath, "*", SearchOption.AllDirectories))
            {
                string folderpath = (Folder.FullPath + "\\" + (Path.GetFileName(folderBrowserDialog.SelectedPath) + "\\" + Path.GetDirectoryName(str).Replace(folderBrowserDialog.SelectedPath, "")).Trim('\\')).Trim('\\');
                string rdaDestFile = RDAFile.FileNameToRDAFileName(str, folderpath);
                RDAFile rdafile = Folder.GetAllFiles().Find(f => f.FileName == rdaDestFile);
                if (rdafile == null)
                {
                    RDAFile rdaFile = RDAFile.Create(Folder.Version, str, folderpath);
                    if (rdaFile != null)
                        files.Add(rdaFile);
                }
                else
                    RDAFileExtension.SetFile(rdafile, str, true);
            }
            Folder.AddFiles(files);
            UpdateSubItems();
        }

        private void context_AddFolderAsRoot_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            List<RDAFile> files = new List<RDAFile>();
            foreach (string str in Directory.GetFiles(folderBrowserDialog.SelectedPath, "*", SearchOption.AllDirectories))
            {
                string folderpath = (Folder.FullPath + "\\" + Path.GetDirectoryName(str).Replace(folderBrowserDialog.SelectedPath, "")).Trim('\\');
                string rdaDestFile = RDAFile.FileNameToRDAFileName(str, folderpath);
                RDAFile rdafile = Folder.GetAllFiles().Find((Predicate<RDAFile>)(f => f.FileName == rdaDestFile));
                if (rdafile == null)
                {
                    RDAFile rdaFile = RDAFile.Create(Folder.Version, str, folderpath);
                    if (rdaFile != null)
                        files.Add(rdaFile);
                }
                else
                    RDAFileExtension.SetFile(rdafile, str, true);
            }
            Folder.AddFiles(files);
            UpdateSubItems();
        }

        private void context_NewFolder_Click(object sender, RoutedEventArgs e)
        {
            string text = MessageWindow.GetText("Folder name:", "New Folder");
            if (text == null)
                return;
            string filename = StringExtension.Replace(text, Path.GetInvalidPathChars(), "").Replace("\\", "").Replace("/", "");
            if (string.IsNullOrEmpty(filename))
                return;
            string str = StringExtension.MakeUnique(filename, "", (f => Folder.Folders.Find(n => n.Name == f) != null));
            Folder.Folders.Add(new RDAFolder(Folder)
            {
                FullPath = Folder.FullPath + "\\" + str,
                Name = str,
            });
            UpdateSubItems();
        }

        private void context_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageWindow.Show("Do you really want to delete this folder?", MessageWindow.MessageWindowType.YesNo) != MessageBoxResult.Yes)
                return;
            Folder.Parent.Folders.Remove(Folder);
            if (Parent == TreeViewExtension.GetTreeView((TreeViewItem)this))
                MainWindow.CurrentMainWindow.RebuildTreeView();
            else
                (Parent as RDAFolderTreeViewItem).UpdateSubItems();
        }
    }
}
