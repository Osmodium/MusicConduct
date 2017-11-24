using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MusicConduct.Controls;

namespace MusicConduct
{
    public partial class MainWindow
    {
        private readonly Brush m_LinkLabelForegroundDefault = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private readonly Brush m_LinkLabelForegroundHighlight = new SolidColorBrush(Color.FromRgb(35, 255, 200));

        private Controls.AboutControl m_AboutControl = null;

        public MainWindow()
        {
            InitializeComponent();

            AboutLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            AboutLinkLabel.MouseLeave += LinkLabel_MouseLeave;
            AboutLinkLabel.MouseUp += AboutLinkLabelOnMouseUp;

            MinimizeLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            MinimizeLinkLabel.MouseLeave += LinkLabel_MouseLeave;
            MinimizeLinkLabel.MouseUp += MinimizeLinkLabelOnMouseUp;

            TitleBarMover.MouseDown += TitleBarDockOnMouseDown;
        }

        private void TitleBarDockOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void MinimizeLinkLabelOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            MessageBox.Show("Minimize");
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
