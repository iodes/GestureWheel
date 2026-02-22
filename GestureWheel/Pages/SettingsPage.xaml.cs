using System;
using System.Windows;
using System.Windows.Controls;
using GestureWheel.Localization;
using GestureWheel.Managers;

namespace GestureWheel.Pages
{
    public partial class SettingsPage
    {
        #region Fields
        private SelectionChangedEventHandler _languageHandler;
        #endregion

        #region Constructor
        public SettingsPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            _languageHandler = delegate
            {
                if (ComboLanguage.SelectedItem is ComboBoxItem item)
                {
                    var code = item.Tag?.ToString() ?? "";
                    SettingsManager.Current.Language = code;
                    SettingsManager.Save();
                    LocalizationManager.Instance.ApplyFromSettings(code);
                }
            };
            ComboLanguage.SelectionChanged += _languageHandler;

            ToggleAutoUpdate.CheckChanged += delegate
            {
                SettingsManager.Current.UseAutoUpdate = ToggleAutoUpdate.IsChecked;
                SettingsManager.Save();
            };

            ToggleAutoStartup.CheckChanged += delegate
            {
                SettingsManager.Current.UseAutoStartup = ToggleAutoStartup.IsChecked;
                SettingsManager.UpdateAutoStartup();
                SettingsManager.Save();
            };
        }
        #endregion

        #region Private Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ToggleAutoUpdate.IsChecked = SettingsManager.Current.UseAutoUpdate;
            ToggleAutoStartup.IsChecked = SettingsManager.Current.UseAutoStartup;

            ComboLanguage.SelectionChanged -= _languageHandler;
            ComboLanguage.SelectedIndex = SettingsManager.Current.Language switch { "en" => 1, "ko" => 2, "ja" => 3, _ => 0 };
            ComboLanguage.SelectionChanged += _languageHandler;
        }
        #endregion
    }
}
