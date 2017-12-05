using MusicConduct.Utility;

namespace MusicConduct.Models
{
    public class Rule
    {
        public RuleType Type { get; set; }
        public ComparisonType Comparison { get; set; }
        public bool IgnoreCase { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
    }
}
