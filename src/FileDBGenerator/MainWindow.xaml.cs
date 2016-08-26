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
        }

        private void textBox_SelectRDAFiles_LostFocus(object sender, RoutedEventArgs e)
        {
            this.SelectRDAFilesFolder(((System.Windows.Controls.TextBox)sender).Text);
        }

        private void button_SelectRDAFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = @"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\games\Anno 2205\maindata";
            if (dialog.ShowDialog(this) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) {
                this.SelectRDAFilesFolder(dialog.FileName);
            }
        }

        private void SelectRDAFilesFolder(string path)
        {
            // TODO check if folder exists and is a folder

            this.viewModel.RDAFilesFolder = path;
            System.Uri pathUri = new System.Uri(path);

            IEnumerable<string> containerPaths = System.IO.Directory.GetFiles(this.viewModel.RDAFilesFolder, @"data*.rda");
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

        private void button_SelectOutputFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonSaveFileDialog();
            dialog.InitialDirectory = this.viewModel.RDAFilesFolder;
            dialog.DefaultFileName = "file.db";
            dialog.Filters.Add(new Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogFilter("Anno 2205 File Index", "*.db"));
            if (dialog.ShowDialog(this) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) {
                this.viewModel.OutputFileName = dialog.FileName;
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

                using (var outputStream = new System.IO.FileStream(this.viewModel.OutputFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
                    using (var writer = new AnnoRDA.FileDB.Writer.FileDBWriter(outputStream, false)) {
                        await Task.Run(() => writer.WriteFileDB(fileSystem, archiveFiles));
                    }
                }
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
