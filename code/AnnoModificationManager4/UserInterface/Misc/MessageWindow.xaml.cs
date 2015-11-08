using System.Windows;
using System.Windows.Input;

namespace AnnoModificationManager4.UserInterface.Misc
{
    public partial class MessageWindow
    {
        public MessageBoxResult result;
        public MessageWindowType MessageType;
        public bool TextInputField_Multiline;

        public MessageWindow()
        {
            InitializeComponent();
        }

        public MessageWindow(MessageWindowType type)
        {
            MessageType = type;
            InitializeComponent();
            switch (MessageType)
            {
                case MessageWindowType.OK:
                    button_ok.Visibility = Visibility.Visible;
                    button_cancel.Visibility = Visibility.Collapsed;
                    button_yes.Visibility = Visibility.Collapsed;
                    button_no.Visibility = Visibility.Collapsed;
                    break;
                case MessageWindowType.OKCancel:
                    button_ok.Visibility = Visibility.Visible;
                    button_cancel.Visibility = Visibility.Visible;
                    button_yes.Visibility = Visibility.Collapsed;
                    button_no.Visibility = Visibility.Collapsed;
                    break;
                case MessageWindowType.YesNo:
                    button_ok.Visibility = Visibility.Collapsed;
                    button_cancel.Visibility = Visibility.Collapsed;
                    button_yes.Visibility = Visibility.Visible;
                    button_no.Visibility = Visibility.Visible;
                    break;
                case MessageWindowType.YesNoCancel:
                    button_ok.Visibility = Visibility.Collapsed;
                    button_cancel.Visibility = Visibility.Visible;
                    button_yes.Visibility = Visibility.Visible;
                    button_no.Visibility = Visibility.Visible;
                    break;
                case MessageWindowType.TextInput:
                    button_ok.Visibility = Visibility.Visible;
                    button_cancel.Visibility = Visibility.Visible;
                    button_yes.Visibility = Visibility.Collapsed;
                    button_no.Visibility = Visibility.Collapsed;
                    TextInputField.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void MessageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.MessageType != MessageWindowType.TextInput)
                return;
            Activate();
            TextInputField.Focus();
            Keyboard.Focus(TextInputField);
            TextInputField.SelectAll();
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            DialogResult = new bool?(true);
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            DialogResult = new bool?(false);
        }

        private void button_yes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            DialogResult = new bool?(true);
        }

        private void button_no_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            DialogResult = new bool?(false);
        }

        public static MessageBoxResult Show(string Message)
        {
            new MessageWindow(MessageWindowType.OK)
            {
                Message = { Text = Message }
            }.ShowDialog();
            return MessageBoxResult.OK;
        }

        public static MessageBoxResult Show(string Message, MessageWindowType tp)
        {
            MessageWindow messageWindow = new MessageWindow(tp);
            messageWindow.Message.Text = Message;
            messageWindow.ShowDialog();
            return messageWindow.result;
        }

        public static string GetText(string Message, string Example, bool MultiLine)
        {
            MessageWindow messageWindow = new MessageWindow(MessageWindowType.TextInput);
            messageWindow.Message.Text = Message;
            messageWindow.TextInputField.Text = Example;
            messageWindow.TextInputField_Multiline = MultiLine;
            bool? nullable = messageWindow.ShowDialog();
            if ((!nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
                return null;
            return messageWindow.TextInputField.Text;
        }

        public static string GetText(string Message, string Example)
        {
            return GetText(Message, Example, false);
        }

        private void TextInputField_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (TextInputField_Multiline || e.Key != Key.Return)
                return;
            DialogResult = new bool?(true);
            e.Handled = true;
        }

        public enum MessageWindowType
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel,
            TextInput,
            XmlInput,
        }
    }
}
