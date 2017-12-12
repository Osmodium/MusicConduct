using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MusicConduct.Events;
using MusicConduct.Models;
using MusicConduct.Utility;

namespace MusicConduct.Controls
{
    /// <summary>
    /// Interaction logic for RuleControl.xaml
    /// </summary>
    public partial class RuleControl : UserControl
    {
        public RuleEvents RuleEvents;
        public RuleControl()
        {
            InitializeComponent();
            RuleEvents = new RuleEvents();
        }

        // TODO edit exiting rules?

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OkButton.IsEnabled = !string.IsNullOrEmpty(ValueText.Text);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RuleEvents.OnCancelRuleCreation(new RuleEvents.CancelRuleCreationEventArgs());
        }

        private static string GetSelectedString(Panel selectorGrid)
        {
            RadioButton selectedType = selectorGrid.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);
            if (selectedType != null)
                return selectedType.DataContext == null ? (string)selectedType.Content : (string)selectedType.DataContext;
            MessageBox.Show("No selected type!");
            return null;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string typeString = GetSelectedString(TypeSelector);
            if (!Enum.TryParse(typeString, true, out RuleType type))
                MessageBox.Show($"Could not convert {typeString} to RuleType...");

            string comparisonString = GetSelectedString(ComparisonSelector);
            if (!Enum.TryParse(comparisonString, true, out ComparisonType comparisonType))
                MessageBox.Show($"Could not convert {comparisonString} to ComparisonType...");

            bool ignoreCase = IgnoreCase.IsChecked.HasValue && IgnoreCase.IsChecked.Value;

            Rule newRule = new Rule
            {
                Value = ValueText.Text,
                Type = type,
                Comparison = comparisonType,
                IgnoreCase = ignoreCase,
                IsActive = true,
            };

            RuleEvents.RuleCreationEventArgs args = new RuleEvents.RuleCreationEventArgs { NewRule = newRule };
            RuleEvents.OnRuleCreation(args);
            ValueText.Text = string.Empty;
        }
    }
}
