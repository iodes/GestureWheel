using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using GestureWheel.Extensions;
using GestureWheel.Localization;
using GestureWheel.Windows.Models;
using Serilog;

namespace GestureWheel.Dialogs
{
    public partial class UpdateDialog
    {
        #region Properties
        private UpdateInfo Info { get; }
        #endregion

        public UpdateDialog(UpdateInfo info)
        {
            Info = info;
            InitializeComponent();

            TextVersion.Text = string.Format(LocalizationManager.Instance["UpdateDialogText"], nameof(GestureWheel), info.Version);
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            BtnUpdate.IsEnabled = false;

            var fileName = Path.Combine(Path.GetTempPath(), Info.FileName);

            using (var client = new HttpClient())
            await using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var progress = new Progress<double>();

                progress.ProgressChanged += (_, value) =>
                {
                    Dispatcher.Invoke(() => ProgressUpdate.Value = value);
                };

                await client.DownloadAsync(Info.Url, file, progress);
            }

            try
            {
                Process.Start(new ProcessStartInfo(fileName)
                {
                    UseShellExecute = true
                });

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating!");
                MessageBox.Show(ex.Message, LocalizationManager.Instance["MsgError"], MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
