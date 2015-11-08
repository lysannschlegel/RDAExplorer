using AnnoModificationManager4.Controls;
using AnnoModificationManager4.Misc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace RDAExplorerGUI.Misc
{
    public static class TreeViewExtension
    {
        public static TreeView GetTreeView(this TreeViewItem item)
        {
            TreeViewItem treeViewItem = item;
            while (!(treeViewItem.Parent is TreeView))
                treeViewItem = treeViewItem.Parent as TreeViewItem;
            return treeViewItem.Parent as TreeView;
        }

        public static string GetNavigator(this ModifiedTreeViewItem item)
        {
            string str = item.SemanticValue;
            if (item.Parent != null && item.Parent is ModifiedTreeViewItem)
                str = GetNavigator(item.Parent as ModifiedTreeViewItem) + "/" + str;
            return str.Trim('/');
        }

        public static ModifiedTreeViewItem NavigateTo(this TreeView view, string path, bool autocreate)
        {
            path = path.Replace("\\", "/");
            List<string> list = Enumerable.ToList(path.Split('/'));
            string message = list[0];
            foreach (ModifiedTreeViewItem view1 in view.Items)
            {
                if (view1.SemanticValue == message)
                {
                    if (list.Count == 1)
                        return view1;
                    list.RemoveAt(0);
                    return NavigateTo(view1, StringExtension.PutTogether(list, '/'), autocreate);
                }
            }
            if (!autocreate)
                return null;
            ModifiedTreeViewItem view2 = new ModifiedTreeViewItem();
            view2.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/folder.png", message);
            view2.SemanticValue = message;
            view.Items.Add((object)view2);
            if (list.Count == 1)
                return view2;
            list.RemoveAt(0);
            return NavigateTo(view2, StringExtension.PutTogether(list, '/'), autocreate);
        }

        private static ModifiedTreeViewItem NavigateTo(ModifiedTreeViewItem view, string path, bool autocreate)
        {
            path = path.Replace("\\", "/");
            List<string> list = Enumerable.ToList(path.Split('/'));
            string message = list[0];
            foreach (ModifiedTreeViewItem view1 in view.Items)
            {
                if (view1.SemanticValue == message)
                {
                    if (list.Count == 1)
                        return view1;
                    list.RemoveAt(0);
                    return NavigateTo(view1, StringExtension.PutTogether(list, '/'), autocreate);
                }
            }
            if (!autocreate)
                return null;
            ModifiedTreeViewItem view2 = new ModifiedTreeViewItem();
            view2.Header = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/folder.png", message);
            view2.SemanticValue = message;
            view.Items.Add(view2);
            if (list.Count == 1)
                return view2;
            list.RemoveAt(0);
            return NavigateTo(view2, StringExtension.PutTogether(list, '/'), autocreate);
        }
    }
}
