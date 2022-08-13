using System;
using System.Collections.Generic;
using System.Linq;
using GestureWheel.Interop;
using GestureWheel.Managers;
using WindowsHook;
using WindowsInput;

namespace GestureWheel.Supports
{
    internal static class GestureSupport
    {
        #region Fields
        private static bool _isWheelDown;
        private static bool _isCtrlDown;
        private static bool _isCanceled;
        private static int _gestureStartX;
        private static int _gestureStartY;
        private static PointerTouchInfo[] _pointers;
        private static IKeyboardMouseEvents _globalHook;
        private static readonly InputSimulator _inputSimulator = new();
        #endregion

        #region Private Methods
        private static IEnumerable<PointerTouchInfo> CreatePointers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var pointer = new PointerTouchInfo();
                pointer.PointerInfo.PointerId = (uint)i;
                pointer.PointerInfo.PointerType = PointerInputType.TOUCH;

                yield return pointer;
            }
        }
        #endregion

        #region Public Methods
        public static bool Start()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseUp += GlobalHook_MouseUp;
            _globalHook.MouseDown += GlobalHook_MouseDown;
            _globalHook.MouseMove += GlobalHook_MouseMove;
            _globalHook.MouseDoubleClick += GlobalHook_MouseDoubleClick;
            _globalHook.KeyDown += GlobalHook_KeyDown;
            _globalHook.KeyUp += GlobalHook_KeyUp;

            return TouchInjector.InitializeTouchInjection(feedbackMode: TouchFeedback.NONE);
        }

        public static void Stop()
        {
            _globalHook.MouseUp -= GlobalHook_MouseUp;
            _globalHook.MouseDown -= GlobalHook_MouseDown;
            _globalHook.MouseMove -= GlobalHook_MouseMove;
            _globalHook.MouseDoubleClick -= GlobalHook_MouseDoubleClick;
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            _globalHook.KeyUp -= GlobalHook_KeyUp;

            _globalHook?.Dispose();
            _globalHook = null;
        }
        #endregion

        #region Mouse Events
        private static void GlobalHook_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button is not MouseButtons.Middle)
                return;

            _isWheelDown = false;
            _isCanceled = false;

            if (_pointers is not null)
            {
                for (int i = 0; i < _pointers.Length; i++)
                {
                    _pointers[i].PointerInfo.PointerFlags = PointerFlags.UP;
                }

                TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
                _pointers = null;
            }

            if (SettingsManager.Current.UseQuickNewDesktop && _isCtrlDown)
            {
                _inputSimulator.Keyboard
                    .ModifiedKeyStroke(new[] { VirtualKeyCode.LWIN, VirtualKeyCode.CONTROL }, VirtualKeyCode.VK_D);
            }
        }

        private static void GlobalHook_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button is not MouseButtons.Middle)
                return;

            _isWheelDown = true;
            _gestureStartX = e.X;
            _gestureStartY = e.Y;
        }

        private static void GlobalHook_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isWheelDown || _isCanceled)
                return;

            var absX = Math.Abs(_gestureStartX - e.X);
            var absY = Math.Abs(_gestureStartY - e.Y);

            var sensitivity = SettingsManager.Current.GestureSensitivity switch
            {
                0 => 10,
                1 => 50,
                2 => 100,
                _ => 50
            };

            switch (_pointers)
            {
                case null when absY >= sensitivity:
                    _isCanceled = true;
                    return;

                case null when absX >= sensitivity:
                {
                    _pointers = CreatePointers(4).ToArray();

                    for (int i = 0; i < _pointers.Length; i++)
                    {
                        _pointers[i].PointerInfo.PtPixelLocation.X = e.X;
                        _pointers[i].PointerInfo.PtPixelLocation.Y = e.Y;
                        _pointers[i].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                    }

                    TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
                    break;
                }
            }

            if (_pointers is not null)
            {
                for (int i = 0; i < _pointers.Length; i++)
                {
                    _pointers[i].PointerInfo.PtPixelLocation.X = e.X;
                    _pointers[i].PointerInfo.PtPixelLocation.Y = e.Y;
                    _pointers[i].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                }

                TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
            }
        }

        private static void GlobalHook_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button is not MouseButtons.Middle)
                return;

            switch (SettingsManager.Current.DoubleClickActionType)
            {
                case 0:
                    break;

                case 1:
                    _inputSimulator.Keyboard
                        .ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.ESCAPE);

                    break;

                case 2:
                    _inputSimulator.Keyboard
                        .ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.TAB);

                    break;
            }
        }

        private static void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData is Keys.LControlKey or Keys.RControlKey)
            {
                _isCtrlDown = true;
            }
        }

        private static void GlobalHook_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData.HasFlag(Keys.LControlKey) || e.KeyData.HasFlag(Keys.RControlKey))
            {
                _isCtrlDown = false;
            }
        }
        #endregion
    }
}
