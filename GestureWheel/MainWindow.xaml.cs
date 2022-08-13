using System.ComponentModel;

namespace GestureWheel
{
    public partial class MainWindow
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            Wpf.Ui.Appearance.Watcher.Watch(this);
            Closing += OnClosing;
        }
        #endregion

        #region Private Events
        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        #endregion
    }
}
