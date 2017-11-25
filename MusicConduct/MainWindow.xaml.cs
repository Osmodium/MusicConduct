using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MusicConduct.Controls;
using MusicConduct.Utility;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace MusicConduct
{
    public partial class MainWindow
    {
        private readonly Brush m_LinkLabelForegroundDefault = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private readonly Brush m_LinkLabelForegroundHighlight = new SolidColorBrush(Color.FromRgb(35, 255, 200));
        private AboutControl m_AboutControl;
        private Icon m_Icon = null;

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

            TitleBarMover.MouseDown += TitleBarDockOnMouseDown;
        }

        private void InitializeNotificationIcon()
        {
            m_Icon = WindowsHelper.GetAppIcon(this);
            if (m_Icon == null)
                return;

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon
            {
                Icon = m_Icon,
                Visible = true
            };
            ni.DoubleClick += delegate
            {
                Show();
                WindowState = WindowState.Normal;
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
            if (m_Icon == null)
                return;
            WindowState = WindowState.Minimized;
        }

        private void AboutLinkLabelOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (m_AboutControl == null)
            {
                m_AboutControl = new AboutControl();
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
            System.Windows.Controls.Label linkLabel = (System.Windows.Controls.Label)sender;
            linkLabel.Foreground = m_LinkLabelForegroundHighlight;
        }

        private void LinkLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Label linkLabel = (System.Windows.Controls.Label)sender;
            linkLabel.Foreground = m_LinkLabelForegroundDefault;
        }

        private void MusicConduct_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LocalSpotifyControl.Dispose();
        }
    }
}
