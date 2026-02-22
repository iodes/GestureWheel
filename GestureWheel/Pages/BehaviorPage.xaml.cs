using System;
using System.Windows;
using GestureWheel.Managers;

namespace GestureWheel.Pages
{
    public partial class BehaviorPage
    {
        public BehaviorPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            ToggleQuickNewDesktop.CheckChanged += delegate
            {
                SettingsManager.Current.UseQuickNewDesktop = ToggleQuickNewDesktop.IsChecked;
                SettingsManager.Save();
            };

            ComboDoubleClickActionType.SelectionChanged += delegate
            {
                SettingsManager.Current.DoubleClickActionType = ComboDoubleClickActionType.SelectedIndex;
                SettingsManager.Save();
            };

            ComboGestureSensitivity.SelectionChanged += delegate
            {
                SettingsManager.Current.GestureSensitivity = ComboGestureSensitivity.SelectedIndex;
                SettingsManager.Save();
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ToggleQuickNewDesktop.IsChecked = SettingsManager.Current.UseQuickNewDesktop;
            ComboDoubleClickActionType.SelectedIndex = Math.Max(0, Math.Min(ComboDoubleClickActionType.Items.Count - 1, SettingsManager.Current.DoubleClickActionType));
            ComboGestureSensitivity.SelectedIndex = Math.Max(0, Math.Min(ComboGestureSensitivity.Items.Count - 1, SettingsManager.Current.GestureSensitivity));
        }
    }
}
