using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GestureWheel.Dialogs;
using GestureWheel.Windows.Models;
using Newtonsoft.Json.Linq;

namespace GestureWheel.Supports
{
    internal static class UpdateSupport
    {
        #region Fields
        private static readonly HttpClient _httpClient = new();
        private static readonly Regex _assetRegex = new(@$"{nameof(GestureWheel)}_(\d+\.?\d+\.?\d+\.?\d+?)_Setup\.exe");
        #endregion

        #region Constants
        private const string repository = "https://api.github.com/repos/iodes/GestureWheel/releases";
        #endregion

        #region Constructor
        static UpdateSupport()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
        }
        #endregion

        #region Public Methods
        public static async Task<bool> CheckUpdateAsync()
        {
            var info = await GetLatestUpdateInfo();

            if (info is null)
                return false;

            var currentVersion = int.Parse(Assembly.GetExecutingAssembly().GetName().Version?.ToString().Replace(".", string.Empty) ?? "0");
            var latestVersion = int.Parse(info.Version?.Replace(".", string.Empty) ?? "0");

            if (latestVersion > currentVersion)
            {
                var updateDialog = new UpdateDialog(info);
                updateDialog.ShowDialog();
            }

            return true;
        }

        public static async Task<UpdateInfo> GetLatestUpdateInfo()
        {
            var json = await _httpClient.GetStringAsync(repository);
            var releases = JArray.Parse(json);

            foreach (var release in releases)
            {
                var info = new UpdateInfo
                {
                    Version = release["tag_name"]?.Value<string>(),
                    Timestamp = release["published_at"]?.Value<DateTime>() ?? default,
                    ReleaseNote = release["body"]?.Value<string>()
                };

                var assets = release["assets"];

                if (assets is null)
                    continue;

                foreach (var asset in assets)
                {
                    var name = asset["name"]?.Value<string>() ?? string.Empty;
                    var nameMatch = _assetRegex.Match(name);

                    if (!nameMatch.Success)
                        continue;

                    info.FileName = name;
                    info.Url = asset["browser_download_url"]?.Value<string>();
                    break;
                }

                if (!string.IsNullOrEmpty(info.Url))
                    return info;
            }

            return null;
        }
        #endregion
    }
}
