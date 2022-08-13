using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace GestureWheel.Controls
{
    internal class CardToggleSwitch : CardControl
    {
        #region Fields
        private readonly ToggleSwitch _toggleSwitch = new();
        #endregion

        #region Events
        public event EventHandler CheckChanged;
        #endregion

        #region Properties
        public bool IsChecked
        {
            get => _toggleSwitch.IsChecked ?? false;
            set => _toggleSwitch.IsChecked = value;
        }
        #endregion

        #region Constructor
        public CardToggleSwitch()
        {
            _toggleSwitch.IsHitTestVisible = false;

            SetResourceReference(StyleProperty, typeof(CardControl));
            AddChild(_toggleSwitch);

            Click += OnClick;
            _toggleSwitch.Checked += ToggleSwitch_CheckChanged;
            _toggleSwitch.Unchecked += ToggleSwitch_CheckChanged;
        }
        #endregion

        #region Private Events
        private void OnClick(object sender, RoutedEventArgs e)
        {
            _toggleSwitch.IsChecked = !_toggleSwitch.IsChecked;
        }

        private void ToggleSwitch_CheckChanged(object sender, RoutedEventArgs e)
        {
            CheckChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
