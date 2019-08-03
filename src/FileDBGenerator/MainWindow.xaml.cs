using FileDBGenerator.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FileDBGenerator
{
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel = new MainWindowViewModel();
        System.Threading.CancellationTokenSource cancellationTokenSource = null;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this.viewModel;

            this.viewModel.RDAFileList.Items.CollectionChanged += ViewModel_RDAFileList_Items_CollectionChanged;
            this.viewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.UpdateGuildingMessage();
        }

        private void UpdateGuildingMessage()
        {
            this.statusBar_textBlock_Message.Text = this.GetGuidingMessage();
        }
        private string GetGuidingMessage()
        {
            if (this.viewModel.RDAFileList.Items.Count() == 0) {
                return @"Select the directory containing the RDA files to start.";
            } else if (this.viewModel.RDAFileList.Items.Count((RDAFileListItem item) => item.IsEnabled) == 0) {
                return @"Enable at least one RDA file.";
            } else if (this.viewModel.OutputFileDB == "") {
                return @"Specify the output file name.";
            } else {
                return @"Click Generate to start the process.";
            }
        }
        private void ViewModel_RDAFileList_Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.UpdateGuildingMessage();
        }
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OutputFileDB") {
                this.UpdateGuildingMessage();
            }
        }

        private void textBox_SelectRDAFiles_LostFocus(object sender, RoutedEventArgs e)
        {
            this.SelectRDAFilesFolder(((System.Windows.Controls.TextBox)sender).Text);
        }

        private void button_SelectRDAFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = this.FindRDAFilesInitialDirectory();
            if (dialog.ShowDialog(this) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) {
                this.SelectRDAFilesFolder(dialog.FileName);
            }
        }

        private string FindRDAFilesInitialDirectory()
        {
            string path = @"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\games\Anno 1800\maindata";
            if (System.IO.Directory.Exists(path)) {
                return path;
            }
            path = @"C:\Program Files\Ubisoft\Ubisoft Game Launcher\games\Anno 1800\maindata";
            if (System.IO.Directory.Exists(path)) {
                return path;
            }
            return null;
        }

        private void SelectRDAFilesFolder(string path)
        {
            if (!System.IO.Directory.Exists(path)) {
                MessageBox.Show(this, "The given path is not a directory.", "Error");
                return;
            }

            this.viewModel.RDAFilesFolder = path;
            System.Uri pathUri = new System.Uri(path);

            IEnumerable<string> containerPaths = System.IO.Directory.GetFiles(this.viewModel.RDAFilesFolder, @"*.rda");
            containerPaths = AnnoRDA.Loader.ContainerDirectoryLoader.SortContainerPaths(containerPaths);
            this.viewModel.RDAFileList.Items.Clear();
            foreach (string filePath in containerPaths) {
                System.Uri filePathUri = new System.Uri(filePath);
                System.Uri relativeFilePathUri = pathUri.MakeRelativeUri(filePathUri);

                this.viewModel.RDAFileList.Items.Add(new RDAFileListItem(true, filePath, relativeFilePathUri.OriginalString));
            }
        }

        private void button_RDAFiles_MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int index = this.viewModel.RDAFileListSelectedIndex;
            if (index > 0) {
                viewModel.RDAFileList.Items.Move(index, index - 1);
            }
        }

        private void button_RDAFiles_MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int index = this.viewModel.RDAFileListSelectedIndex;
            if (index > -1 && index + 1 < viewModel.RDAFileList.Items.Count) {
                viewModel.RDAFileList.Items.Move(index, index + 1);
            }
        }

        private void button_SelectOutputFileDB_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonSaveFileDialog();
            dialog.InitialDirectory = this.viewModel.RDAFilesFolder;
            dialog.DefaultFileName = "file.db";
            dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Anno 2205/1800 File Index", "*.db"));
            if (dialog.ShowDialog(this) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) {
                this.viewModel.OutputFileDB = dialog.FileName;
            }
        }

        private void button_SelectOutputChecksumDB_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonSaveFileDialog();
            dialog.InitialDirectory = this.viewModel.RDAFilesFolder;
            dialog.DefaultFileName = "checksum.db";
            dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Anno 1800 checksum file", "*.db"));
            if (dialog.ShowDialog(this) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) {
                this.viewModel.OutputChecksumDB = dialog.FileName;
            }
        }

        private async void button_Generate_Click(object sender, RoutedEventArgs e)
        {
            this.cancellationTokenSource = new System.Threading.CancellationTokenSource();
            this.viewModel.IsGenerating = true;

            ICollection<RDAFileListItem> enabledItems = viewModel.RDAFileList.Items.Where((item) => item.IsEnabled).ToList();

            int numSteps = enabledItems.Count * 2; // reading
            numSteps += 1; // writing

            this.statusBar_progressBar_Progress.Minimum = 0;
            this.statusBar_progressBar_Progress.Maximum = numSteps;
            this.statusBar_progressBar_Progress.Value = 0;

            var archiveFiles = new AnnoRDA.FileDB.Writer.ArchiveFileMap();
            var fileSystem = new AnnoRDA.FileSystem();
            var fileLoader = new AnnoRDA.Loader.ContainerFileLoader();

            try {
                foreach (var rdaFile in enabledItems) {
                    this.statusBar_textBlock_Message.Text = System.String.Format("Loading {0}", rdaFile.Name);
                    archiveFiles.Add(rdaFile.LoadPath, rdaFile.Name);

                    var progress = new System.Progress<string>((string fileName) => {
                        this.statusBar_textBlock_Message.Text = System.String.Format("Loading {0}: {1}", rdaFile.Name, fileName);
                    });
                    var containerFileSystem = await Task.Run(() => fileLoader.Load(rdaFile.LoadPath, progress, this.cancellationTokenSource.Token));
                    this.statusBar_progressBar_Progress.Value += 1;

                    this.statusBar_textBlock_Message.Text = System.String.Format("Loading {0}", rdaFile.Name);
                    await Task.Run(() => fileSystem.OverwriteWith(containerFileSystem, null, this.cancellationTokenSource.Token));
                    this.statusBar_progressBar_Progress.Value += 1;
                }

                this.statusBar_textBlock_Message.Text = "Writing...";
                await Task.Run(() => {
                    using (var fileDBStream = new System.IO.FileStream(this.viewModel.OutputFileDB, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite)) {
                        using (var fileDBWriter = new AnnoRDA.FileDB.Writer.FileSystemWriter(fileDBStream, true)) {
                            fileDBWriter.WriteFileSystem(fileSystem, archiveFiles);
                        }

                        if (this.viewModel.OutputChecksumDB != "") {
                            fileDBStream.Position = 0;
                            byte[] checksum = AnnoRDA.ChecksumDB.Generator.ComputeChecksum(fileDBStream);
                            fileDBStream.Dispose();

                            using (var checksumDBStream = new System.IO.FileStream(this.viewModel.OutputChecksumDB, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
                                using (var checksumDBWriter = new System.IO.BinaryWriter(checksumDBStream)) {
                                    checksumDBWriter.Write(checksum);
                                }
                            }
                        }
                    }
                });
                this.statusBar_progressBar_Progress.Value += 1;

                this.statusBar_textBlock_Message.Text = "Done";

            } catch(System.OperationCanceledException) {
                this.statusBar_textBlock_Message.Text = "Canceled";

            } finally {
                this.viewModel.IsGenerating = false;
                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }
        }

        private void statusBar_button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}
