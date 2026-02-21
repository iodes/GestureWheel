using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace GestureWheel.Localization
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        public static LocalizationManager Instance { get; } = new();

        private CultureInfo _culture = CultureInfo.GetCultureInfo("en");

        private static readonly ResourceManager ResourceManager =
            new("GestureWheel.Localization.Strings", typeof(LocalizationManager).Assembly);

        public event PropertyChangedEventHandler PropertyChanged;

        public string this[string key] =>
            ResourceManager.GetString(key, _culture) ?? key;

        public void SetLanguage(string code)
        {
            _culture = code switch
            {
                "ko" => CultureInfo.GetCultureInfo("ko"),
                "ja" => CultureInfo.GetCultureInfo("ja"),
                _ => CultureInfo.GetCultureInfo("en")
            };
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));
        }

        public void ApplyFromSettings(string savedCode)
        {
            if (string.IsNullOrEmpty(savedCode))
            {
                savedCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
                {
                    "ko" => "ko",
                    "ja" => "ja",
                    _ => "en"
                };
            }
            SetLanguage(savedCode);
        }
    }
}
