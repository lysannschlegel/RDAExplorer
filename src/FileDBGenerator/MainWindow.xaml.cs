using FileDBGenerator.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FileDBGenerator
{
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this.viewModel;
        }

        private void textBox_SelectRDAFiles_LostFocus(object sender, RoutedEventArgs e)
        {
            this.SelectRDAFilesFolder(this.textBox_SelectRDAFiles.Text);
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
            int index = this.listView_RDAFiles.SelectedIndex;
            if (index > 0) {
                viewModel.RDAFileList.Items.Move(index, index - 1);
            }
        }

        private void button_RDAFiles_MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int index = this.listView_RDAFiles.SelectedIndex;
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
                this.textBox_SelectOutputFile.Text = dialog.FileName;
            }
        }

        private void button_Generate_Click(object sender, RoutedEventArgs e)
        {
            var archiveFiles = new AnnoRDA.FileDB.Writer.ArchiveFileMap();
            var fileSystem = new AnnoRDA.FileSystem();
            var fileLoader = new AnnoRDA.Loader.ContainerFileLoader();

            foreach (var rdaFile in viewModel.RDAFileList.Items) {
                if (rdaFile.IsEnabled) {
                    archiveFiles.Add(rdaFile.LoadPath, rdaFile.Name);
                    var containerFileSystem = fileLoader.Load(rdaFile.LoadPath);
                    fileSystem.Merge(containerFileSystem, System.Threading.CancellationToken.None);
                }
            }

            using (var outputStream = new System.IO.FileStream(this.textBox_SelectOutputFile.Text, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
                using (var writer = new AnnoRDA.FileDB.Writer.FileDBWriter(outputStream, false)) {
                    writer.WriteFileDB(fileSystem, archiveFiles);
                }
            }
        }
    }
}
