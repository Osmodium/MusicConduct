using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using MusicConduct.Utility;

namespace MusicConduct.Controls
{
    public partial class AboutControl : UserControl, IDisposable
    {
        public event CloseAboutHandler CloseAbout;
        public delegate void CloseAboutHandler(AboutControl aboutControl);

        public AboutControl(string clientVersionString = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(clientVersionString))
                ClientVersionText.Text = $"Spotify Client Version: {clientVersionString}";

            GithubBwImage.MouseEnter += BwImageOnMouseEnter;
            GithubBwImage.MouseLeave += BwImageOnMouseLeave;
            GithubBwImage.MouseUp += GithubBwImageOnMouseUp;

            TwitterBwImage.MouseEnter += BwImageOnMouseEnter;
            TwitterBwImage.MouseLeave += BwImageOnMouseLeave;
            TwitterBwImage.MouseUp += TwitterBwImageOnMouseUp;

            CloseAboutGrid.MouseUp += CloseAboutGridOnMouseUp;
        }

        private void CloseAboutGridOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            CloseAbout?.Invoke(this);
        }

        private static void GithubBwImageOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Process.Start("https://github.com/Osmodium/MusicConduct");
        }

        private static void TwitterBwImageOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Process.Start("https://twitter.com/Osmodium");
        }

        private static void BwImageOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            if (sender is Image image)
            {
                image.AnimateOpacity(0, 1000);
            }
        }

        private static void BwImageOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (sender is Image image)
            {
                image.AnimateOpacity(1, 500);
            }
        }

        public void Dispose()
        {
            // Do dispose stuff
        }
    }
}
