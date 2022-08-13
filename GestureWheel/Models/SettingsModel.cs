using Newtonsoft.Json;

namespace GestureWheel.Models
{
    internal class SettingsModel
    {
        [JsonProperty]
        public bool UseAutoStartup { get; set; } = true;

        [JsonProperty]
        public bool UseQuickNewDesktop { get; set; } = true;

        [JsonProperty]
        public int DoubleClickActionType { get; set; } = 2;

        [JsonProperty]
        public int GestureSensitivity { get; set; } = 1;
    }
}
