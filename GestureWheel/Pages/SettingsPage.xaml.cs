using System;
using System.Windows;
using GestureWheel.Managers;

namespace GestureWheel.Pages
{
    public partial class SettingsPage
    {
        #region Constructor
        public SettingsPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            ToggleAutoStartup.CheckChanged += delegate
            {
                SettingsManager.Current.UseAutoStartup = ToggleAutoStartup.IsChecked;
                SettingsManager.UpdateAutoStartup();
                SettingsManager.Save();
            };

            ToggleStartMenuOpen.CheckChanged += delegate
            {
                SettingsManager.Current.UseStartMenuOpen = ToggleStartMenuOpen.IsChecked;
                SettingsManager.Save();
            };

            ToggleQuickNewDesktop.CheckChanged += delegate
            {
                SettingsManager.Current.UseQuickNewDesktop = ToggleQuickNewDesktop.IsChecked;
                SettingsManager.Save();
            };

            ComboGestureSensitivity.SelectionChanged += delegate
            {
                SettingsManager.Current.GestureSensitivity = ComboGestureSensitivity.SelectedIndex;
                SettingsManager.Save();
            };
        }
        #endregion

        #region Private Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ToggleAutoStartup.IsChecked = SettingsManager.Current.UseAutoStartup;
            ToggleStartMenuOpen.IsChecked = SettingsManager.Current.UseStartMenuOpen;
            ToggleQuickNewDesktop.IsChecked = SettingsManager.Current.UseQuickNewDesktop;
            ComboGestureSensitivity.SelectedIndex = Math.Max(0, Math.Min(ComboGestureSensitivity.Items.Count - 1, SettingsManager.Current.GestureSensitivity));
        }
        #endregion
    }
}
