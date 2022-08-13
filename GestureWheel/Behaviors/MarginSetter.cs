using System.Windows;
using System.Windows.Controls;

namespace GestureWheel.Behaviors
{
    internal class MarginSetter
    {
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter), new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }

        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }

        public static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            panel.Loaded -= OnPanelLoaded;
            panel.Loaded += OnPanelLoaded;
            OnPanelLoaded(sender, null);
        }

        private static void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            foreach (var child in panel.Children)
            {
                if (child is not FrameworkElement fe)
                    continue;

                fe.Margin = GetMargin(panel);
            }
        }
    }
}
