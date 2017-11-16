using System.Windows;
using System.Windows.Controls;

namespace MusicNope.Controls
{
    /// <summary>
    /// Interaction logic for LoaderMessageControl.xaml
    /// </summary>
    public partial class LoaderMessageControl : UserControl
    {
        private LoaderMessageControl()
        {
            InitializeComponent();
        }

        public static LoaderMessageControl ShowLoader(string message)
        {
            LoaderMessageControl newLoaderMessageControl = new LoaderMessageControl
            {
                ControlGrid =
                {
                    IsEnabled = false,
                    Visibility = Visibility.Hidden
                },
                MessageTextBlock = {Text = message}
            };
            return newLoaderMessageControl;
        }

        public static LoaderMessageControl ShowDialog(string message)
        {
            LoaderMessageControl newLoaderMessageControl = new LoaderMessageControl
            {
                LoaderGrid =
                {
                    IsEnabled = false,
                    Visibility = Visibility.Hidden
                },
                MessageTextBlock = {Text = message}
            };
            return newLoaderMessageControl;
        }

        public void SetMessage(string message)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => SetMessage(message));
                return;
            }
            MessageTextBlock.Text = message;
        }
    }
}
