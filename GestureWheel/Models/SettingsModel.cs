using Newtonsoft.Json;

namespace GestureWheel.Models
{
    internal class SettingsModel
    {
        [JsonProperty]
        public bool UseAutoStartup { get; set; } = true;

        [JsonProperty]
        public bool UseStartMenuOpen { get; set; }

        [JsonProperty]
        public int GestureSensitivity { get; set; } = 1;
    }
}
