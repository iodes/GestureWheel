using System;
using System.Reflection;
using System.Windows;
using GestureWheel.Localization;
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
                {
                    var loc = LocalizationManager.Instance;
                    MessageBox.Show(loc["MsgAlreadyLatest"], loc["MsgNoUpdate"], MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during manual update checking!");
                var loc = LocalizationManager.Instance;
                MessageBox.Show(loc["MsgUpdateFailed"], loc["MsgUpdateFailedTitle"], MessageBoxButton.OK, MessageBoxImage.Warning);
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
