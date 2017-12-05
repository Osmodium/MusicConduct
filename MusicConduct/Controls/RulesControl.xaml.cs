using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MusicConduct.Events;
using MusicConduct.Models;
using MusicConduct.Properties;
using MusicConduct.Utility;
using Newtonsoft.Json;
using SpotifyAPI.Local.Models;

namespace MusicConduct.Controls
{
    /// <summary>
    /// Interaction logic for RulesControl.xaml
    /// </summary>
    public partial class RulesControl : UserControl, IDisposable
    {
        private readonly List<Rule> m_Rules;
        public RulesControl()
        {
            InitializeComponent();
            try
            {
                m_Rules = JsonConvert.DeserializeObject<List<Rule>>(Settings.Default.Rules) ?? new List<Rule>();
                PopulateRulesCheckboxes();
            }
            catch (Exception)
            {
                m_Rules = new List<Rule>();
            }

            SkipExplicitSongsCheckBox.IsChecked = Settings.Default.SkipExplicitSongs;
            SkipRepeatedSongsCheckBox.IsChecked = Settings.Default.SkipRepeatingSongs;
            RepeatedSongTimeSlider.Value = Settings.Default.RepeatingSongsTimespan;

            NewRuleControl.RuleEvents.RuleCreation += RulesControl_RuleCreation;
            NewRuleControl.RuleEvents.CancelRuleCreation += RuleEventsOnCancelRuleCreation;
        }

        private void RuleEventsOnCancelRuleCreation(object sender, RuleEvents.CancelRuleCreationEventArgs e)
        {
            DisableNewRule();
        }

        private void RulesControl_RuleCreation(object sender, RuleEvents.RuleCreationEventArgs e)
        {
            m_Rules.Add(e.NewRule);
            AddRuleCheckbox(e.NewRule);
            DisableNewRule();
        }

        private void PopulateRulesCheckboxes()
        {
            foreach (Rule rule in m_Rules)
            {
                AddRuleCheckbox(rule);
            }
        }

        private void AddRuleCheckbox(Rule rule)
        {
            string type = Enum.GetName(typeof(RuleType), rule.Type);
            string comp = Enum.GetName(typeof(ComparisonType), rule.Comparison);
            if (comp != null)
            {
                if (comp.Equals("startswith", StringComparison.InvariantCultureIgnoreCase))
                    comp = "starts with";
                if (comp.Equals("endswith", StringComparison.InvariantCultureIgnoreCase))
                    comp = "ends with";
            }
            string content = $"{type} {comp} {rule.Value}";
            CheckBox checkBox = new CheckBox
            {
                Content = content,
                IsChecked = rule.IsActive,
                DataContext = rule
            };
            checkBox.Checked += CheckBoxCheckChanged;
            checkBox.Unchecked += CheckBoxCheckChanged;
            RulesList.Items.Add(checkBox);
        }

        private static void CheckBoxCheckChanged(object sender, RoutedEventArgs e)
        {
            CheckBox currentCheckBox = (CheckBox)sender;
            if (currentCheckBox.IsChecked.HasValue)
                ((Rule)currentCheckBox.DataContext).IsActive = currentCheckBox.IsChecked.Value;
        }
        public bool ShouldSkipTrack(Track track)
        {
            return m_Rules.Any(rule => TestTrack(track, rule));
        }

        private static bool TestTrack(Track track, Rule rule)
        {
            if (!rule.IsActive)
                return false;

            switch (rule.Type)
            {
                case RuleType.Artist:
                return Compare(track.ArtistResource.Name, rule);
                case RuleType.Album:
                return Compare(track.AlbumResource.Name, rule);
                case RuleType.Track:
                return Compare(track.TrackResource.Name, rule);
                default:
                return false;
            }
        }

        private static bool Compare(string value, Rule rule)
        {
            if (rule.IgnoreCase)
            {
                value = value.ToLowerInvariant();
                rule.Value = rule.Value.ToLowerInvariant();
            }

            switch (rule.Comparison)
            {
                case ComparisonType.Equals:
                return value.Equals(rule.Value);
                case ComparisonType.Contains:
                return value.Contains(rule.Value);
                case ComparisonType.StartsWith:
                return value.StartsWith(rule.Value);
                case ComparisonType.EndsWith:
                return value.EndsWith(rule.Value);
                default:
                return false;
            }
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            EnableNewRule();
        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem))
                return;
            if (!(menuItem.CommandParameter is CheckBox currentCheckbox))
                return;
            Rule rule = (Rule)currentCheckbox.DataContext;
            m_Rules.Remove(rule);
            currentCheckbox.Checked -= CheckBoxCheckChanged;
            currentCheckbox.Unchecked -= CheckBoxCheckChanged;
            RulesList.Items.Remove(currentCheckbox);
        }

        private void EnableNewRule()
        {
            NewRuleControl.IsEnabled = true;
            ToggleRulesListColumn(0, RulesGrid.ActualWidth);
        }

        private void DisableNewRule()
        {
            ToggleRulesListColumn(RulesGrid.ActualWidth, 0, () => { NewRuleControl.IsEnabled = false; });
        }

        private void ToggleRulesListColumn(double from, double to, Action onCompleted = null)
        {
            GridLengthAnimation animationTimeline = new GridLengthAnimation
            {
                From = new GridLength(from),
                To = new GridLength(to),
                Duration = TimeSpan.FromMilliseconds(250)
            };
            if (onCompleted != null)
                animationTimeline.Completed += delegate
                {
                    onCompleted();
                };
            RuleColumn.BeginAnimation(ColumnDefinition.WidthProperty, animationTimeline);
        }

        public void Dispose()
        {
            Settings.Default.RepeatingSongsTimespan = RepeatedSongTimeSlider.Value;
            if (SkipExplicitSongsCheckBox.IsChecked.HasValue)
                Settings.Default.SkipExplicitSongs = SkipExplicitSongsCheckBox.IsChecked.Value;
            if (SkipRepeatedSongsCheckBox.IsChecked.HasValue)
                Settings.Default.SkipRepeatingSongs = SkipRepeatedSongsCheckBox.IsChecked.Value;

            Settings.Default.Rules = JsonConvert.SerializeObject(m_Rules);

            Settings.Default.Save();

            NewRuleControl.RuleEvents.RuleCreation -= RulesControl_RuleCreation;
            NewRuleControl.RuleEvents.CancelRuleCreation -= RuleEventsOnCancelRuleCreation;
        }
    }
}
