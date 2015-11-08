using AnnoModificationManager4.UserInterface.Misc;
using RDAExplorer;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RDAExplorerGUI
{
    public partial class SaveRDAFileWindow
    {
        public RDAFolder Folder;
        public FileHeader.Version SelectedVersion;

        public SaveRDAFileWindow()
        {
            InitializeComponent();
        }

        private static string GetVersionString(FileHeader.Version version)
        {
            switch (version)
            {
                case FileHeader.Version.Version_2_0: return "2.0 (Anno 1404 & 2070)";
                case FileHeader.Version.Version_2_2: return "2.2 (Anno 2205)";
            }
            return "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedVersion = Folder.Version == FileHeader.Version.Invalid ? FileHeader.Version.Version_2_2 : Folder.Version;

            // add version RadioButtons
            foreach (var version in (FileHeader.Version[])Enum.GetValues(typeof(FileHeader.Version)))
            {
                if (version != FileHeader.Version.Invalid)
                {
                    RadioButton radioButton = new RadioButton();
                    radioButton.GroupName = "version";
                    radioButton.Content = GetVersionString(version);
                    radioButton.Tag = version;
                    radioButton.Checked += radioButton_Version_Checked;

                    if (SelectedVersion == version)
                        radioButton.IsChecked = true;

                    versionsPanel.Children.Add(radioButton);
                }
            }

            // pre-select compression for version 2.0
            check_IsCompressed.IsChecked = SelectedVersion == FileHeader.Version.Version_2_0;

            // add compression CheckBoxes
            foreach (string str in Folder.GetAllExtensions())
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Content = str;
                if (RDABlockCreator.FileType_CompressedExtensions.Contains(str))
                    checkBox.IsChecked = new bool?(true);
                compressedTypesPanel.Children.Add(checkBox);
            }
        }

        private void radioButton_Version_Checked(object sender, RoutedEventArgs e)
        {
            SelectedVersion = (FileHeader.Version)((RadioButton)sender).Tag;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVersion == FileHeader.Version.Invalid)
            {
                MessageWindow.Show("Please select a version.");
                return;
            }

            foreach (CheckBox checkBox in this.compressedTypesPanel.Children)
            {
                string str = checkBox.Content.ToString();
                bool? isChecked = checkBox.IsChecked;
                if ((!isChecked.GetValueOrDefault() ? 0 : (isChecked.HasValue ? 1 : 0)) != 0)
                {
                    if (!RDABlockCreator.FileType_CompressedExtensions.Contains(str))
                        RDABlockCreator.FileType_CompressedExtensions.Add(str);
                }
                else if (RDABlockCreator.FileType_CompressedExtensions.Contains(str))
                    RDABlockCreator.FileType_CompressedExtensions.Remove(str);
            }

            DialogResult = new bool?(true);
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = new bool?(false);
        }
    }
}
