using GestureWheel.Supports;

namespace GestureWheel
{
    public partial class MainWindow
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            GestureSupport.Start();
        }
        #endregion
    }
}
