using System.Windows;
using Wpf.Ui.Controls;

namespace GestureWheel.Controls
{
    internal class CardToggleSwitch : CardControl
    {
        #region Fields
        private readonly ToggleSwitch _toggleSwitch = new();
        #endregion

        #region Properties
        public bool IsChecked => _toggleSwitch.IsChecked ?? false;
        #endregion

        #region Constructor
        public CardToggleSwitch()
        {
            _toggleSwitch.IsHitTestVisible = false;

            SetResourceReference(StyleProperty, typeof(CardControl));
            AddChild(_toggleSwitch);

            Click += OnClick;
        }
        #endregion

        #region Private Events
        private void OnClick(object sender, RoutedEventArgs e)
        {
            _toggleSwitch.IsChecked = !_toggleSwitch.IsChecked;
        }
        #endregion
    }
}
