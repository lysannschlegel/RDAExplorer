using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AnnoModificationManager4.Misc
{
    public class ControlExtension
    {
        public static object BuildImageTextblock(string file, string message)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            try
            {
                stackPanel.Children.Add(new Image()
                {
                    Source = BitmapImageExtension.Load(file),
                    Stretch = Stretch.None
                });
            }
            catch (Exception)
            {
            }
            UIElementCollection children = stackPanel.Children;
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = message;
            textBlock1.Margin = new Thickness(5.0, 0.0, 0.0, 0.0);
            TextBlock textBlock2 = textBlock1;
            children.Add(textBlock2);
            return stackPanel;
        }

        public static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);
            return source;
        }
    }
}