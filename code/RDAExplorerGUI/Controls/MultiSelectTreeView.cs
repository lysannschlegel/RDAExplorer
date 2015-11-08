using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RDAExplorerGUI.Controls
{
    public class MultiSelectTreeView : TreeView
    {
        public static readonly DependencyProperty AutoRecursiveProperty = DependencyProperty.Register("AutoRecursive", typeof(bool), typeof(MultiSelectTreeView), (PropertyMetadata)new UIPropertyMetadata((object)false));
        public List<object> SelectedItems = new List<object>();

        public bool AutoRecursive
        {
            get
            {
                return (bool)GetValue(AutoRecursiveProperty);
            }
            set
            {
                SetValue(AutoRecursiveProperty, value);
            }
        }

        public List<object> AllItems
        {
            get
            {
                List<object> list = new List<object>();
                foreach (object obj in Items)
                    list.AddRange(GetRecursiveItems(obj));
                return list;
            }
        }

        public MultiSelectTreeView()
        {
            SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(MultiSelectTreeView_SelectedItemChanged);
        }

        private List<object> GetRecursiveItems(object item)
        {
            List<object> list = new List<object>();
            list.Add(item);
            if (item is TreeViewItem)
            {
                foreach (object obj in (item as TreeViewItem).Items)
                    list.AddRange(GetRecursiveItems(obj));
            }
            return list;
        }

        public void UpdateSelectedItems()
        {
            foreach (object obj in AllItems)
            {
                if (obj is TreeViewItem)
                    ((FrameworkElement)obj).Style = SelectedItems.Contains(obj) ? Application.Current.Resources["TreeViewItemStyle_Selected"] as Style : null;
            }
        }

        public void SelectItem(object item)
        {
            if (!SelectedItems.Contains(item))
                SelectedItems.Add(item);
            if (!(item is TreeViewItem) || !this.AutoRecursive)
                return;
            foreach (object obj in (item as TreeViewItem).Items)
                SelectItem(obj);
        }

        private void MultiSelectTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    SelectItem(SelectedItem);
                }
                else
                {
                    SelectedItems.Clear();
                    SelectItem(SelectedItem);
                }
            }
            else
                SelectedItems.Clear();
            UpdateSelectedItems();
        }
    }
}
