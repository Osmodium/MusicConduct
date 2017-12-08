using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MusicConduct.Controls;
using MusicConduct.Events;
using MusicConduct.Utility;
using Label = System.Windows.Controls.Label;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MusicConduct
{
    public partial class MainWindow
    {
        private AboutControl m_AboutControl;
        private NotifyIcon m_NotifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNotificationIcon();

            AboutLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            AboutLinkLabel.MouseLeave += LinkLabel_MouseLeave;
            AboutLinkLabel.MouseUp += AboutLinkLabelOnMouseUp;

            MinimizeLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            MinimizeLinkLabel.MouseLeave += LinkLabel_MouseLeave;
            MinimizeLinkLabel.MouseUp += MinimizeLinkLabelOnMouseUp;

            //FoldupLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            //FoldupLinkLabel.MouseLeave += LinkLabel_MouseLeave;
            //FoldupLinkLabel.MouseUp += FoldupLinkLabelOnMouseUp;

            TitleBarMover.MouseDown += TitleBarDockOnMouseDown;

            LocalSpotifyControl.SpotifyLocalEvents.TrackChanged += SpotifyLocalEventsOnTrackChanged;
        }

        private void SpotifyLocalEventsOnTrackChanged(object o, SpotifyLocalEvents.TrackChangeEventArgs e)
        {
            Retry.Do(() =>
            {
                string text = $"{e.Title} by {e.Artist}";
                if (text.Length > 64)
                    text = text.Substring(0, 60) + "...";
                m_NotifyIcon.Text = text;
            }, TimeSpan.FromMilliseconds(1000), 10);

        }

        private void InitializeNotificationIcon()
        {
            m_NotifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.AppIcon,
                Visible = true
            };
            m_NotifyIcon.DoubleClick += delegate
            {
                ShowApp();
            };
            m_NotifyIcon.MouseDown += (sender, args) =>
            {
                if (args.Button != MouseButtons.Right)
                    return;
                System.Windows.Controls.ContextMenu menu = (System.Windows.Controls.ContextMenu)FindResource("NotifierContextMenu");
                menu.IsOpen = true;
            };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
            base.OnStateChanged(e);
        }

        private void TitleBarDockOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void MinimizeLinkLabelOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (m_NotifyIcon == null)
                return;
            WindowState = WindowState.Minimized;
        }

        //private void FoldupLinkLabelOnMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    double to = Math.Abs(MusicConduct.ActualHeight - 125) < 0.1f ? 440 : 125;
        //    DoubleAnimation animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(250));
        //    MusicConduct.BeginAnimation(HeightProperty, animation);
        //}

        private void Menu_Show(object sender, RoutedEventArgs e)
        {
            ShowApp();
        }

        private void Menu_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowApp()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        private void AboutLinkLabelOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (m_AboutControl == null)
            {
                m_AboutControl = new AboutControl(LocalSpotifyControl.SpotifyClientVersion);
                ContentGrid.Children.Add(m_AboutControl);
                m_AboutControl.CloseAbout += AboutControl_CloseAbout;
            }
            else
            {
                AboutControl_CloseAbout(m_AboutControl);
            }
        }

        private void AboutControl_CloseAbout(AboutControl aboutControl)
        {
            if (aboutControl.Equals(m_AboutControl))
            {
                m_AboutControl.Dispose();
                ContentGrid.Children.Remove(m_AboutControl);
                m_AboutControl = null;
                return;
            }
            aboutControl.Dispose();
        }

        private void LinkLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            Label linkLabel = (Label)sender;
            linkLabel.Foreground = Constants.LinkLabelForegroundHighlight;
        }

        private void LinkLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            Label linkLabel = (Label)sender;
            linkLabel.Foreground = Constants.LinkLabelForegroundBlack;
        }

        private void MusicConduct_Closing(object sender, CancelEventArgs e)
        {
            LocalSpotifyControl.Dispose();
            m_NotifyIcon.Dispose();
        }
    }
}
