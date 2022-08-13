using System.Windows;

namespace GestureWheel
{
    public partial class MainWindow
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            Wpf.Ui.Appearance.Watcher.Watch(this);
        }
        #endregion
    }
}
