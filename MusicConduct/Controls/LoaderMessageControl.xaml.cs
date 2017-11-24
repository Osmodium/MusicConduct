using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;

namespace MusicConduct.Controls
{
    /// <summary>
    /// Interaction logic for LoaderMessageControl.xaml
    /// </summary>
    public partial class LoaderMessageControl : IDisposable
    {
        public float ColorLerpDuration = 500;
        private readonly List<string> m_ColorHexes = new List<string>();
        private int m_CurrentTargetColorHexIndex;
        private readonly SolidColorBrush m_AnimationColorBrush = new SolidColorBrush();

        private LoaderMessageControl()
        {
            InitializeComponent();
            m_ColorHexes.Add("#33FF0000");
            m_ColorHexes.Add("#33FFFF00");
            m_ColorHexes.Add("#3300FF00");
            m_ColorHexes.Add("#3300FFFF");
            m_ColorHexes.Add("#330000FF");
            m_ColorHexes.Add("#33FF00FF");

            RegisterName("AnimationColorBrush", m_AnimationColorBrush);

            LoaderImageTint.Fill = m_AnimationColorBrush;

            LerpTingColor();
        }

        private void LerpTingColor()
        {
            Color fromColor = ((SolidColorBrush)LoaderImageTint.Fill).Color;
            
            m_AnimationColorBrush.Color = fromColor;
            
            SolidColorBrush toColorBrush = (SolidColorBrush) new BrushConverter().ConvertFrom(m_ColorHexes[m_CurrentTargetColorHexIndex]);
            if (toColorBrush == null)
                return;
            Color toColor = toColorBrush.Color;
            
            ColorAnimation colorAnimation = new ColorAnimation(fromColor, toColor, new Duration(TimeSpan.FromMilliseconds(ColorLerpDuration)));
            colorAnimation.Completed += (o, args) =>
            {
                m_CurrentTargetColorHexIndex = (m_CurrentTargetColorHexIndex + 1) % m_ColorHexes.Count;
                LerpTingColor();
            };

            Storyboard.SetTargetName(colorAnimation, "AnimationColorBrush");
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));
            Storyboard colorStoryboard = new Storyboard();
            colorStoryboard.Children.Add(colorAnimation);
            colorStoryboard.Begin(this);
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

        public void Dispose()
        {

        }
    }
}
