using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FileDBGenerator.Collections.ObjectModel
{
    class ObservableCollectionEx<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public ObservableCollectionEx()
        {
            this.CollectionChanged += ObservableCollectionEx_CollectionChanged;
        }

        private void ObservableCollectionEx_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null) {
                foreach (object item in e.OldItems) {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
                }
            }
            if (e.NewItems != null) {
                foreach (object item in e.NewItems) {
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                action: NotifyCollectionChangedAction.Replace,
                newItem: sender,
                oldItem: sender,
                index: this.IndexOf((T)sender)
            ));
        }
    }
}
