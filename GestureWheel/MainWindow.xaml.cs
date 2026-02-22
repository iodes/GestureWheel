using System.ComponentModel;
using System.Windows;
using GestureWheel.Pages;

namespace GestureWheel
{
    public partial class MainWindow
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
            Loaded += OnLoaded;
            Closing += OnClosing;
        }
        #endregion

        #region Private Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate(typeof(BehaviorPage));
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        #endregion
    }
}
