using Newtonsoft.Json;

namespace GestureWheel.Models
{
    internal class Settings
    {
        [JsonProperty]
        public bool UseAutoUpdate { get; set; } = true;

        [JsonProperty]
        public bool UseAutoStartup { get; set; } = true;

        [JsonProperty]
        public bool UseQuickNewDesktop { get; set; } = true;

        [JsonProperty]
        public int DoubleClickActionType { get; set; } = 2;

        [JsonProperty]
        public int GestureSensitivity { get; set; } = 1;

        [JsonProperty]
        public string Language { get; set; } = "";
    }
}
