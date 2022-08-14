using System;
using System.IO;
using GestureWheel.Models;
using GestureWheel.Supports;
using Microsoft.Win32;
using Newtonsoft.Json;
using Serilog;

namespace GestureWheel.Managers
{
    internal static class SettingsManager
    {
        #region Fields
        private static Settings _current;
        #endregion

        #region Properties
        public static Settings Current
        {
            get
            {
                if (_current is null)
                    Load();

                return _current;
            }
        }
        #endregion

        #region Public Methods
        public static void Load()
        {
            if (!File.Exists(EnvironmentSupport.Settings))
            {
                _current = new Settings();
                Save();
            }

            try
            {
                var settingsText = File.ReadAllText(EnvironmentSupport.Settings);

                if (string.IsNullOrEmpty(settingsText.Trim()))
                    throw new InvalidOperationException("The contents of the settings file are missing!");

                _current = JsonConvert.DeserializeObject<Settings>(settingsText);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while loading settings!");
                _current = new Settings();
                Save();
            }
        }

        public static void UpdateAutoStartup()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (key is null)
                    return;

                if (Current.UseAutoStartup)
                {
                    key.SetValue(nameof(GestureWheel), $"\"{EnvironmentSupport.Executable}\"");
                }
                else
                {
                    key.DeleteValue(nameof(GestureWheel), false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to register or delete startup programs!");
            }
        }

        public static void Save()
        {
            var settingsText = JsonConvert.SerializeObject(_current);

            if (!string.IsNullOrEmpty(settingsText.Trim()))
                File.WriteAllText(EnvironmentSupport.Settings, settingsText);
        }
        #endregion
    }
}
