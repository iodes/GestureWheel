using System.Diagnostics;
using System.IO;
using GestureWheel.Models;
using GestureWheel.Supports;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GestureWheel.Managers
{
    internal static class SettingsManager
    {
        #region Fields
        private static SettingsModel _current;
        #endregion

        #region Properties
        public static SettingsModel Current
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
                _current = new SettingsModel();
                Save();
            }

            var settingsText = File.ReadAllText(EnvironmentSupport.Settings);
            _current = JsonConvert.DeserializeObject<SettingsModel>(settingsText);
        }

        public static void UpdateAutoStartup()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (key is null)
                return;

            if (Current.UseAutoStartup)
            {
                key.SetValue(nameof(GestureWheel), $"\"{Process.GetCurrentProcess().MainModule?.FileName}\"");
            }
            else
            {
                key.DeleteValue(nameof(GestureWheel), false);
            }
        }

        public static void Save()
        {
            var settingsText = JsonConvert.SerializeObject(_current);
            File.WriteAllText(EnvironmentSupport.Settings, settingsText);
        }
        #endregion
    }
}
