using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
            m_ColorHexes.Add("#CCFF0000");
            m_ColorHexes.Add("#CCFFFF00");
            m_ColorHexes.Add("#CC00FF00");
            m_ColorHexes.Add("#CC00FFFF");
            m_ColorHexes.Add("#CC0000FF");
            m_ColorHexes.Add("#CCFF00FF");

            RegisterName("AnimationColorBrush", m_AnimationColorBrush);

            ProgressRing.Foreground = m_AnimationColorBrush;

            LerpTintColor();
        }

        private void LerpTintColor()
        {
            Color fromColor = ((SolidColorBrush)ProgressRing.Foreground).Color;
            
            m_AnimationColorBrush.Color = fromColor;
            
            SolidColorBrush toColorBrush = (SolidColorBrush) new BrushConverter().ConvertFrom(m_ColorHexes[m_CurrentTargetColorHexIndex]);
            if (toColorBrush == null)
                return;
            Color toColor = toColorBrush.Color;
            
            ColorAnimation colorAnimation = new ColorAnimation(fromColor, toColor, new Duration(TimeSpan.FromMilliseconds(ColorLerpDuration)));
            colorAnimation.Completed += (o, args) =>
            {
                m_CurrentTargetColorHexIndex = (m_CurrentTargetColorHexIndex + 1) % m_ColorHexes.Count;
                LerpTintColor();
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
