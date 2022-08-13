using System.Collections.Generic;
using System.Linq;
using GestureWheel.Interop;
using WindowsHook;

namespace GestureWheel.Supports
{
    internal static class GestureSupport
    {
        #region Fields
        private static bool _isWheelDown;
        private static PointerTouchInfo[] _pointers;
        private static IKeyboardMouseEvents _globalHook;
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

            return TouchInjector.InitializeTouchInjection(feedbackMode: TouchFeedback.NONE);
        }

        public static void Stop()
        {
            _globalHook.MouseUp -= GlobalHook_MouseUp;
            _globalHook.MouseDown -= GlobalHook_MouseDown;
            _globalHook.MouseMove -= GlobalHook_MouseMove;
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

            for (int i = 0; i < _pointers.Length; i++)
            {
                _pointers[i].PointerInfo.PointerFlags = PointerFlags.UP;
            }

            TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
        }

        private static void GlobalHook_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button is not MouseButtons.Middle)
                return;

            _isWheelDown = true;

            _pointers = CreatePointers(4).ToArray();

            for (int i = 0; i < _pointers.Length; i++)
            {
                _pointers[i].PointerInfo.PtPixelLocation.X = e.X;
                _pointers[i].PointerInfo.PtPixelLocation.Y = e.Y;
                _pointers[i].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            }

            TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
        }

        private static void GlobalHook_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isWheelDown)
                return;

            for (int i = 0; i < _pointers.Length; i++)
            {
                _pointers[i].PointerInfo.PtPixelLocation.X = e.X;
                _pointers[i].PointerInfo.PtPixelLocation.Y = e.Y;
                _pointers[i].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            }

            TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
        }
        #endregion
    }
}
