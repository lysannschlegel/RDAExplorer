using AnnoModificationManager4.Misc;
using AnnoModificationManager4.UserInterface.Misc;
using Microsoft.VisualBasic.FileIO;
using RDAExplorer;
using RDAExplorerGUI.Misc;
using RDAExplorerGUI.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace RDAExplorerGUI
{
    public partial class MainWindow
    {
        public RDAReader CurrentReader = new RDAReader();
        public string CurrentFileName = "";
        public List<RDAFile> FileWatcher_ToUpdate = new List<RDAFile>();
        public static MainWindow CurrentMainWindow;
        public FileSystemWatcher FileWatcher;
        public bool FileWatcher_Updating;

        public MainWindow()
        {
            CurrentMainWindow = this;
            InitializeComponent();
            Width = Settings.Default.Window_Width;
            Height = Settings.Default.Window_Height;
            Left = Settings.Default.Window_X;
            Top = Settings.Default.Window_Y;
            WindowState = Settings.Default.Window_IsMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NewFile();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Settings.Default.Window_Width = Width;
                Settings.Default.Window_Height = Height;
                Settings.Default.Window_X = Left;
                Settings.Default.Window_Y = Top;
            }
            Settings.Default.Window_IsMaximized = WindowState == WindowState.Maximized;
            Settings.Default.Save();
            ResetDocument();
            try
            {
                Directory.Delete(DirectoryExtension.GetTempWorkingDirectory(), true);
            }
            catch (Exception)
            {
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            FileWatcher_ToUpdate = Enumerable.ToList(Enumerable.Distinct(FileWatcher_ToUpdate));
            if (FileWatcher_ToUpdate.Count == 0 || FileWatcher_Updating)
                return;
            string str1 = "Following files has changed:\n";
            foreach (RDAFile rdaFile in FileWatcher_ToUpdate)
                str1 = str1 + rdaFile.FileName + "\n";
            string Message = str1 + "\nDo you want to update the RDA File Items?";
            FileWatcher_Updating = true;
            if (MessageWindow.Show(Message, MessageWindow.MessageWindowType.YesNo) == MessageBoxResult.Yes)
            {
                foreach (RDAFile rdaFile in FileWatcher_ToUpdate)
                {
                    string str2 = DirectoryExtension.GetTempWorkingDirectory() + "\\" + rdaFile.FileName;
                    string str3 = StringExtension.MakeUnique(Path.ChangeExtension(str2, null) + "$", Path.GetExtension(str2), (d => Directory.Exists(d)));
                    File.Copy(str2, str3);
                    rdaFile.SetFile(str3);
                }
            }
            FileWatcher_Updating = false;
            FileWatcher_ToUpdate.Clear();
        }

        public BackgroundWorker RebuildTreeView()
        {
            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            progressBar_Status.Visibility = Visibility.Visible;
            wrk.ProgressChanged += (s, e) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
            {
                progressBar_Status.Value = e.ProgressPercentage;
                label_Status.Text = "Updating UI";
            });
            wrk.DoWork += (s, e) => _RebuildTreeView(wrk);
            wrk.RunWorkerCompleted += (s, e) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () => progressBar_Status.Visibility = Visibility.Collapsed);
            wrk.RunWorkerAsync();

            return wrk;
        }

        private void _RebuildTreeView(BackgroundWorker wrk)
        {
            DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
            {
                treeView.Items.Clear();
                RDAFolder rdaFolder1 = CurrentReader.rdaFolder;
                foreach (RDAFolder rdaFolder2 in rdaFolder1.Folders)
                    treeView.Items.Add(new RDAFolderTreeViewItem()
                    {
                        Folder = rdaFolder2,
                        Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/folder.png", rdaFolder2.Name)
                    });
                foreach (RDAFile file in rdaFolder1.Files)
                    treeView.Items.Add(Misc.RDAFileExtension.ToTreeViewItem(file));
            });
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
                string generatedRDAFileName = RDAFile.FileNameToRDAFileName(file, CurrentReader.rdaFolder.FullPath);
                RDAFile rdafile = CurrentReader.rdaFolder.Files.Find((Predicate<RDAFile>)(f => f.FileName == generatedRDAFileName));
                if (rdafile == null)
                {
                    RDAFile rdaFile = RDAFile.Create(file, CurrentReader.rdaFolder.FullPath);
                    if (rdaFile != null)
                        CurrentReader.rdaFolder.Files.Add(rdaFile);
                }
                else
                    RDAExplorerGUI.Misc.RDAFileExtension.SetFile(rdafile, file, true);
            }
            RebuildTreeView();
        }

        private void context_AddFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            List<RDAFile> files = new List<RDAFile>();
            foreach (string str in Directory.GetFiles(folderBrowserDialog.SelectedPath, "*", System.IO.SearchOption.AllDirectories))
            {
                string folderpath = (CurrentReader.rdaFolder.FullPath + "\\" + (Path.GetFileName(folderBrowserDialog.SelectedPath) + "\\" + Path.GetDirectoryName(str).Replace(folderBrowserDialog.SelectedPath, "")).Trim('\\')).Trim('\\');
                string rdaDestFile = RDAFile.FileNameToRDAFileName(str, folderpath);
                RDAFile rdafile = CurrentReader.rdaFolder.GetAllFiles().Find(f => f.FileName == rdaDestFile);
                if (rdafile == null)
                {
                    RDAFile rdaFile = RDAFile.Create(str, folderpath);
                    if (rdaFile != null)
                        files.Add(rdaFile);
                }
                else
                    Misc.RDAFileExtension.SetFile(rdafile, str, true);
            }
            CurrentReader.rdaFolder.AddFiles(files);
            RebuildTreeView();
        }

        private void context_AddFolderAsRoot_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            List<RDAFile> files = new List<RDAFile>();
            foreach (string str in Directory.GetFiles(folderBrowserDialog.SelectedPath, "*", System.IO.SearchOption.AllDirectories))
            {
                string folderpath = (CurrentReader.rdaFolder.FullPath + "\\" + Path.GetDirectoryName(str).Replace(folderBrowserDialog.SelectedPath, "")).Trim('\\');
                string rdaDestFile = RDAFile.FileNameToRDAFileName(str, folderpath);
                RDAFile rdafile = CurrentReader.rdaFolder.GetAllFiles().Find(f => f.FileName == rdaDestFile);
                if (rdafile == null)
                {
                    RDAFile rdaFile = RDAFile.Create(str, folderpath);
                    if (rdaFile != null)
                        files.Add(rdaFile);
                }
                else
                    Misc.RDAFileExtension.SetFile(rdafile, str, true);
            }
            CurrentReader.rdaFolder.AddFiles(files);
            RebuildTreeView();
        }

        private void context_NewFolder_Click(object sender, RoutedEventArgs e)
        {
            string text = MessageWindow.GetText("Folder name:", "New Folder");
            if (text == null)
                return;
            string filename = StringExtension.Replace(text, Path.GetInvalidPathChars(), "").Replace("\\", "").Replace("/", "");
            if (string.IsNullOrEmpty(filename))
                return;
            string str = StringExtension.MakeUnique(filename, "", f => CurrentReader.rdaFolder.Folders.Find(n => n.Name == f) != null);
            CurrentReader.rdaFolder.Folders.Add(new RDAFolder(CurrentReader.rdaFolder)
            {
                FullPath = "\\" + str,
                Name = str,
            });
            RebuildTreeView();
        }

        private void file_New_Click(object sender, RoutedEventArgs e)
        {
            NewFile();
        }

        private void file_OpenReadOnly_Click(object sender, RoutedEventArgs e)
        {
            AnnoModificationManager4.Misc.OpenFileDialog openFileDialog = new AnnoModificationManager4.Misc.OpenFileDialog();
            openFileDialog.Filter = "Valid Files|*.rda;*.sww;*.rdu|All files|*.*";
            bool? nullable = openFileDialog.ShowDialog();
            if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                return;
            OpenFile(openFileDialog.FileName, true);
        }

        private void file_Open_Click(object sender, RoutedEventArgs e)
        {
            AnnoModificationManager4.Misc.OpenFileDialog openFileDialog = new AnnoModificationManager4.Misc.OpenFileDialog();
            openFileDialog.Filter = "Valid Files|*.rda;*.sww;*.rdu|All files|*.*";
            bool? nullable = openFileDialog.ShowDialog();
            if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                return;
            OpenFile(openFileDialog.FileName, false);
        }

        private void file_Save_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentReader.rdaFolder.GetAllFiles().Count == 0)
            {
                int num1 = (int)MessageWindow.Show("Cannot save an empty file!");
            }
            else if (string.IsNullOrEmpty(CurrentFileName))
            {
                file_SaveAs_Click(null, null);
            }
            else
            {
                SaveRDAFileWindow saveRdaFileWindow = new SaveRDAFileWindow();
                saveRdaFileWindow.Folder = CurrentReader.rdaFolder;
                saveRdaFileWindow.field_OutputFile.Text = CurrentFileName;
                bool? nullable = saveRdaFileWindow.ShowDialog();
                if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                    return;
                string fileName = saveRdaFileWindow.field_OutputFile.Text;
                bool compress = saveRdaFileWindow.check_IsCompressed.IsChecked.Value;
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                RDAWriter writer = new RDAWriter(CurrentReader.rdaFolder);
                BackgroundWorker wrk = new BackgroundWorker();
                wrk.WorkerReportsProgress = true;
                progressBar_Status.Visibility = Visibility.Visible;
                wrk.ProgressChanged += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
                {
                    label_Status.Text = writer.UI_LastMessage;
                    progressBar_Status.Value = e2.ProgressPercentage;
                });
                wrk.RunWorkerCompleted += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
                {
                    label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                    progressBar_Status.Visibility = Visibility.Collapsed;
                });
                wrk.DoWork += (s, e2) =>
                {
                    try
                    {
                        writer.Write(fileName, saveRdaFileWindow.SelectedVersion, compress, wrk);
                    }
                    catch (Exception ex)
                    {
                        int num2 = (int)DispatcherExtension.Dispatch(System.Windows.Application.Current, () => MessageWindow.Show(ex.Message));
                    }
                };
                wrk.RunWorkerAsync();
            }
        }

        private void file_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentReader.rdaFolder.GetAllFiles().Count == 0)
            {
                int num1 = (int)MessageWindow.Show("Cannot save an empty file!");
            }
            else
            {
                AnnoModificationManager4.Misc.SaveFileDialog saveFileDialog = new AnnoModificationManager4.Misc.SaveFileDialog();
                saveFileDialog.Filter = "RDA File|*.rda|Savegame|*.sww|Scenario|*.rdu";
                bool? nullable1 = saveFileDialog.ShowDialog();
                if ((!nullable1.GetValueOrDefault() ? 0 : (nullable1.HasValue ? 1 : 0)) == 0)
                    return;
                SaveRDAFileWindow saveRdaFileWindow = new SaveRDAFileWindow();
                saveRdaFileWindow.Folder = CurrentReader.rdaFolder;
                saveRdaFileWindow.field_OutputFile.Text = saveFileDialog.FileName;
                bool? nullable2 = saveRdaFileWindow.ShowDialog();
                if ((!nullable2.GetValueOrDefault() ? 0 : (nullable2.HasValue ? 1 : 0)) == 0)
                    return;
                string fileName = saveRdaFileWindow.field_OutputFile.Text;
                CurrentFileName = fileName;
                bool compress = saveRdaFileWindow.check_IsCompressed.IsChecked.Value;
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                RDAWriter writer = new RDAWriter(CurrentReader.rdaFolder);
                BackgroundWorker wrk = new BackgroundWorker();
                wrk.WorkerReportsProgress = true;
                progressBar_Status.Visibility = Visibility.Visible;
                wrk.ProgressChanged += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
                {
                    label_Status.Text = writer.UI_LastMessage;
                    progressBar_Status.Value = e2.ProgressPercentage;
                });
                wrk.RunWorkerCompleted += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
                {
                    label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                    progressBar_Status.Visibility = Visibility.Collapsed;
                });
                wrk.DoWork += (s, e2) =>
                {
                    try
                    {
                        writer.Write(fileName, saveRdaFileWindow.SelectedVersion, compress, wrk);
                    }
                    catch (Exception ex)
                    {
                        int num2 = (int)DispatcherExtension.Dispatch(System.Windows.Application.Current, () => MessageWindow.Show(ex.Message));
                    }
                };
                wrk.RunWorkerAsync();
            }
        }

        private void file_Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void archive_ExtractAll_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            progressBar_Status.Visibility = Visibility.Visible;
            wrk.ProgressChanged += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
            {
                label_Status.Text = RDAExplorer.RDAFileExtension.ExtractAll_LastMessage;
                progressBar_Status.Value = e2.ProgressPercentage;
            });
            wrk.RunWorkerCompleted += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
            {
                label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                progressBar_Status.Visibility = Visibility.Collapsed;
            });
            wrk.DoWork += (s, e2) =>
            {
                try
                {
                    RDAExplorer.RDAFileExtension.ExtractAll(CurrentReader.rdaFolder.GetAllFiles(), dlg.SelectedPath, wrk);
                }
                catch (Exception ex1)
                {
                    Exception ex = ex1;
                    int num = (int)DispatcherExtension.Dispatch(System.Windows.Application.Current, () => MessageWindow.Show(ex.Message));
                }
            };
            wrk.RunWorkerAsync();
        }

        private void archive_ExtractSelected_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            BackgroundWorker wrk = new BackgroundWorker();
            wrk.WorkerReportsProgress = true;
            progressBar_Status.Visibility = Visibility.Visible;
            wrk.ProgressChanged += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
            {
                label_Status.Text = RDAExplorer.RDAFileExtension.ExtractAll_LastMessage;
                progressBar_Status.Value = e2.ProgressPercentage;
            });
            wrk.RunWorkerCompleted += (s, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
            {
                label_Status.Text = CurrentReader.rdaFolder.GetAllFiles().Count + " files";
                progressBar_Status.Visibility = Visibility.Collapsed;
            });
            wrk.DoWork += (s, e2) =>
            {
                try
                {
                    List<RDAFile> list = new List<RDAFile>();
                    foreach (RDAFileTreeViewItem fileTreeViewItem in Enumerable.OfType<RDAFileTreeViewItem>(treeView.SelectedItems))
                        list.Add(fileTreeViewItem.File);
                    foreach (RDAFolderTreeViewItem folderTreeViewItem in Enumerable.OfType<RDAFolderTreeViewItem>(treeView.SelectedItems))
                        list.AddRange(folderTreeViewItem.Folder.GetAllFiles());
                    RDAExplorer.RDAFileExtension.ExtractAll(Enumerable.ToList(Enumerable.Distinct(list)), dlg.SelectedPath, wrk);
                }
                catch (Exception ex1)
                {
                    Exception ex = ex1;
                    int num = (int)DispatcherExtension.Dispatch(System.Windows.Application.Current, () => MessageWindow.Show(ex.Message));
                }
            };
            wrk.RunWorkerAsync();
        }

        private void archive_SearchFile_Click(object sender, RoutedEventArgs e)
        {
            string text = MessageWindow.GetText("Search File with Name", "File.ext");
            if (text == null)
                return;
            foreach (RDAFolderTreeViewItem folderTreeViewItem in Enumerable.OfType<RDAFolderTreeViewItem>(treeView.Items))
            {
                RDAFileTreeViewItem fileTreeViewItem = folderTreeViewItem.SearchFile(text);
                if (fileTreeViewItem != null)
                {
                    fileTreeViewItem.IsSelected = true;
                    break;
                }
            }
        }

        private void archive_SearchFolder_Click(object sender, RoutedEventArgs e)
        {
            string text = MessageWindow.GetText("Search Folder with Name", "Folder");
            if (text == null)
                return;
            foreach (RDAFolderTreeViewItem folderTreeViewItem1 in Enumerable.OfType<RDAFolderTreeViewItem>(treeView.Items))
            {
                RDAFolderTreeViewItem folderTreeViewItem2 = folderTreeViewItem1.SearchFolder(text);
                if (folderTreeViewItem2 != null)
                {
                    folderTreeViewItem2.IsSelected = true;
                    break;
                }
            }
        }

        private void button_Filter_Refresh_Click(object sender, RoutedEventArgs e)
        {
            RebuildTreeView();
        }

        private void ResetDocument()
        {
            CurrentFileName = "";
            file_Save.IsEnabled = true;
            if (FileWatcher != null)
                FileWatcher.Dispose();
            FileWatcher = new FileSystemWatcher();
            FileWatcher.IncludeSubdirectories = true;
            FileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            CurrentReader.Dispose();
            DirectoryExtension.CleanDirectory(DirectoryExtension.GetTempWorkingDirectory());
            FileWatcher.Path = DirectoryExtension.GetTempWorkingDirectory();
            FileWatcher.EnableRaisingEvents = true;
        }

        private void NewFile()
        {
            Title = GetTitle();
            label_Status.Text = "";
            ResetDocument();
            CurrentReader = new RDAReader();
            RebuildTreeView();
        }

        private void OpenFile(string fileName, bool isreadonly)
        {
            RDAReader reader = new RDAReader();
            ResetDocument();
            CurrentFileName = fileName;
            if (!isreadonly)
                fileName = DirectoryExtension.GetTempWorkingDirectory() + "\\" + Path.GetFileName(fileName);
            else
                file_Save.IsEnabled = false;
            CurrentReader = reader;
            reader.FileName = fileName;
            progressBar_Status.Visibility = Visibility.Visible;
            Title = GetTitle() + " - " + Path.GetFileName(reader.FileName);
            reader.backgroundWorker = new BackgroundWorker();
            reader.backgroundWorker.WorkerReportsProgress = true;
            reader.backgroundWorker.ProgressChanged += (sender2, e2) => DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
            {
                progressBar_Status.Value = e2.ProgressPercentage;
                label_Status.Text = reader.backgroundWorkerLastMessage;
            });
            reader.backgroundWorker.DoWork += (sender2, e2) =>
            {
                try
                {
                    if (!isreadonly)
                    {
                        DispatcherExtension.Dispatch(System.Windows.Application.Current, () => label_Status.Text = "Copying *.rda file to a temparary directory ...");
                        FileSystem.CopyFile(CurrentFileName, fileName, UIOption.AllDialogs, UICancelOption.ThrowException);
                    }
                    reader.ReadRDAFile();
                }
                catch (Exception ex)
                {
                    DispatcherExtension.Dispatch(System.Windows.Application.Current, () =>
                    {
                        int num = (int)MessageWindow.Show(ex.Message);
                        NewFile();
                    });
                }
            };
            reader.backgroundWorker.RunWorkerCompleted += (sender2, e2) =>
            {
                progressBar_Status.Visibility = Visibility.Collapsed;

                var treeWorker = RebuildTreeView();
                if (reader.rdaSkippedBlocks > 0)
                {
                    treeWorker.RunWorkerCompleted += (wrk, e) =>
                    {
                        MessageWindow.Show(reader.rdaSkippedBlocks + " blocks with " + reader.rdaSkippedFiles + " files could not be read. This data will be missing when saving the file!");
                    };
                }
            };
            reader.backgroundWorker.RunWorkerAsync();
        }

        public string GetTitle()
        {
            return "Anno 1404/2070/2205 RDA Explorer Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
