using System;
using System.ComponentModel;
using System.Linq;

namespace FileDBGenerator.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string rdaFilesFolder = "";
        public string RDAFilesFolder {
            get {
                return this.rdaFilesFolder;
            }
            set {
                this.rdaFilesFolder = value;
                this.NotifyPropertyChanged("RDAFilesFolder");
            }
        }

        public RDAFileList RDAFileList { get; }
        private int rdaFileListSelectedIndex = -1;
        public int RDAFileListSelectedIndex {
            get {
                return this.rdaFileListSelectedIndex;
            }
            set {
                this.rdaFileListSelectedIndex = value;
                this.NotifyPropertyChanged("RDAFilesMoveUpButtonEnabled");
                this.NotifyPropertyChanged("RDAFilesMoveDownButtonEnabled");
            }
        }
        public bool? RDAFileListAllEnabled {
            get {
                bool anyEnabled = false;
                bool allEnabled = true;
                foreach (RDAFileListItem item in this.RDAFileList.Items) {
                    if (item.IsEnabled) {
                        anyEnabled = true;
                    } else {
                        allEnabled = false;
                    }
                }
                if (allEnabled) {
                    return true;
                } else if (anyEnabled) {
                    return null;
                } else {
                    return false;
                }
            }
            set {
                if (!value.HasValue) {
                    throw new ArgumentException("value must be present");
                }
                foreach (RDAFileListItem item in this.RDAFileList.Items) {
                    item.IsEnabled = value.Value;
                }
                this.NotifyPropertyChanged("RDAFileListAllEnabled");
            }
        }

        public bool RDAFilesMoveUpButtonEnabled { get {
                return this.RDAFileListSelectedIndex > 0;
        } }
        public bool RDAFilesMoveDownButtonEnabled { get {
                return this.RDAFileListSelectedIndex > -1 && this.RDAFileListSelectedIndex + 1 < this.RDAFileList.Items.Count;
        } }

        private string outputFileDB = "";
        public string OutputFileDB {
            get {
                return this.outputFileDB;
            }
            set {
                this.outputFileDB = value;
                this.NotifyPropertyChanged("OutputFileDB");
                this.NotifyPropertyChanged("GenerateButtonEnabled");
            }
        }

        private string outputChecksumDB = "";
        public string OutputChecksumDB {
            get {
                return this.outputChecksumDB;
            }
            set {
                this.outputChecksumDB = value;
                this.NotifyPropertyChanged("OutputChecksumDB");
            }
        }

        public bool GenerateButtonEnabled { get {
                return this.RDAFileList.Items.Any((RDAFileListItem item) => item.IsEnabled) &&
                       this.OutputFileDB != "";
        } }

        private bool isGenerating = false;
        public bool IsGenerating {
            get {
                return this.isGenerating;
            }
            set {
                this.isGenerating = value;
                this.NotifyPropertyChanged("IsGenerating");
            }
        }

        public MainWindowViewModel()
        {
            this.RDAFileList = new RDAFileList();
            this.RDAFileList.Items.CollectionChanged += RDAFileList_Items_CollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RDAFileList_Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset
            ) {
                this.NotifyPropertyChanged("RDAFilesMoveDownButtonEnabled");
                this.NotifyPropertyChanged("RDAFileListAllEnabled");
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace) {
                this.NotifyPropertyChanged("GenerateButtonEnabled");
                this.NotifyPropertyChanged("RDAFileListAllEnabled");
            }
        }
    }

    class MockMainWindowViewModel : MainWindowViewModel
    {
        public MockMainWindowViewModel()
        {
            this.RDAFilesFolder = @"C:\Foobar\maindata";

            this.RDAFileList.Items.Add(new RDAFileListItem(true, @"Dummy", @"maindata/data0.rda"));
            this.RDAFileList.Items.Add(new RDAFileListItem(false, @"Dummy", "maindata/data12.rda"));
            this.RDAFileListSelectedIndex = 0;

            this.OutputFileDB = @"C:\Foobar\maindata\file.db";
            this.OutputChecksumDB = @"C:\Foobar\maindata\checksum.db";
        }
    }
}
