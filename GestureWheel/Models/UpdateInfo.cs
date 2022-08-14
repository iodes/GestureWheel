using System;

namespace GestureWheel.Windows.Models
{
    public class UpdateInfo
    {
        public string Version { get; set; }

        public DateTime Timestamp { get; set; }

        public string ReleaseNote { get; set; }

        public string FileName { get; set; }

        public string Url { get; set; }
    }
}
