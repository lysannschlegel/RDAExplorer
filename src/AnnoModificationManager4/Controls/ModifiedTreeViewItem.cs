using AnnoModificationManager4.Misc;
using System;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnnoModificationManager4.Controls
{
    public class ModifiedTreeViewItem : TreeViewItem
    {
        public static readonly DependencyProperty SemanticValueProperty = DependencyProperty.Register("SemanticValue", typeof(string), typeof(ModifiedTreeViewItem), new UIPropertyMetadata(""));
        public static readonly DependencyProperty SelectOnRightClickProperty = DependencyProperty.Register("SelectOnRightClick", typeof(bool), typeof(ModifiedTreeViewItem), new UIPropertyMetadata(false));

        public string SemanticValue
        {
            get
            {
                return (string)GetValue(SemanticValueProperty);
            }
            set
            {
                SetValue(SemanticValueProperty, value);
            }
        }

        public bool SelectOnRightClick
        {
            get
            {
                return (bool)GetValue(SelectOnRightClickProperty);
            }
            set
            {
                SetValue(SelectOnRightClickProperty, value);
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (SelectOnRightClick)
                IsSelected = true;
            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (e.Action != NotifyCollectionChangedAction.Remove || e.OldStartingIndex <= 0 || Items.Count == 0)
                return;
            new Thread((ParameterizedThreadStart)delegate
            {
                Thread.Sleep(75);
                DispatcherExtension.Dispatch<Application>(Application.Current, app => (Items[e.OldStartingIndex - 1] as TreeViewItem).IsSelected = true);
            }).Start();
        }
    }
}
