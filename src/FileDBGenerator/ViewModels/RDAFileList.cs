using FileDBGenerator.Collections.ObjectModel;
using System.ComponentModel;

namespace FileDBGenerator.ViewModels
{
    class RDAFileListItem : INotifyPropertyChanged
    {
        private bool isEnabled;
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set {
                this.isEnabled = value;
                this.NotifyPropertyChanged("IsEnabled");
            }
        }
        public string LoadPath { get; }
        public string Name { get; }

        internal RDAFileListItem(bool isEnabled, string loadPath, string name)
        {
            this.IsEnabled = isEnabled;
            this.LoadPath = loadPath;
            this.Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class RDAFileList
    {
        public ObservableCollectionEx<RDAFileListItem> Items { get; }

        public RDAFileList()
        {
            this.Items = new ObservableCollectionEx<RDAFileListItem>();
        }
    }
}
