using System.Windows.Media;

namespace MusicConduct.Utility
{
    public enum RuleType
    {
        Artist,
        Album,
        Track
    }

    public enum ComparisonType
    {
        Equals,
        Contains,
        StartsWith,
        EndsWith
    }

    public static class Constants
    {
        public static readonly Brush LinkLabelForegroundWhite = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static readonly Brush LinkLabelForegroundBlack = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        public static readonly Brush LinkLabelForegroundHighlight = new SolidColorBrush(Color.FromRgb(35, 255, 200));
    }
}
