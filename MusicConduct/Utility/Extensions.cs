using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MusicConduct.Utility
{
    public static class Extensions
    {
        public static void AnimateOpacity(this IAnimatable animatable, double toOpacity, double time)
        {
            DoubleAnimation animation = new DoubleAnimation(toOpacity, new Duration(TimeSpan.FromMilliseconds(time)));
            animatable.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public static void ChangeSource(this Image image, ImageSource source, TimeSpan fadeOutTime, TimeSpan fadeInTime)
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation(1d, fadeInTime);

            if (image.Source != null)
            {
                DoubleAnimation fadeOutAnimation = new DoubleAnimation(0d, fadeOutTime);

                fadeOutAnimation.Completed += (o, e) =>
                {
                    image.Source = source;
                    image.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                };

                image.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                image.Opacity = 0d;
                image.Source = source;
                image.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            }
        }
    }
}
