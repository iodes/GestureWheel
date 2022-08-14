using System;
using System.Reflection;
using System.Windows;
using GestureWheel.Supports;
using GestureWheel.Utilities;
using Serilog;

namespace GestureWheel.Pages
{
    public partial class AboutPage
    {
        #region Constructor
        public AboutPage()
        {
            InitializeComponent();
            TextVersion.Text = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        }
        #endregion

        #region Private Events
        private async void BtnCheckUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            BtnCheckUpdate.IsEnabled = false;

            try
            {
                if (!await UpdateSupport.CheckUpdateAsync())
                    MessageBox.Show("이미 최신 버전을 사용하고 있습니다.", "업데이트 없음", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during manual update checking!");
                MessageBox.Show("업데이트 확인을 실패했습니다.\n잠시 후 다시 시도해주세요.", "업데이트 확인 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                BtnCheckUpdate.IsEnabled = true;
            }
        }

        private void BtnOpenHomepage_OnClick(object sender, RoutedEventArgs e)
        {
            UrlUtility.Open("https://github.com/iodes/GestureWheel");
        }
        #endregion
    }
}
