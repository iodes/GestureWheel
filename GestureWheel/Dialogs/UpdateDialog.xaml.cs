using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using GestureWheel.Extensions;
using GestureWheel.Windows.Models;

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

            TextVersion.Text = $"새로운 버전이 출시되었습니다.\n{nameof(GestureWheel)}을 {info.Version} 버전으로 업데이트 하시겠습니까?";
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
                MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
