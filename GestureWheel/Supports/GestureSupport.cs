using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Automation;
using GestureWheel.Interop;
using GestureWheel.Managers;
using WindowsHook;
using WindowsInput;
using System.Runtime.InteropServices;
using Log = Serilog.Log;

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
        private static volatile bool _hasHorizontalScrollAtClick;
        private static Task _horizontalScrollCheckTask;
        #endregion

        #region Properties
        private static bool IsNativeGestureSupport { get; set; }
        #endregion

        #region Native Methods
        internal enum QUERY_USER_NOTIFICATION_STATE
        {
            QUNS_NOT_PRESENT = 1,
            QUNS_BUSY = 2,
            QUNS_RUNNING_D3D_FULL_SCREEN = 3,
            QUNS_PRESENTATION_MODE = 4,
            QUNS_ACCEPTS_NOTIFICATIONS = 5,
            QUNS_QUIET_TIME = 6,
            QUNS_APP = 7
        }

        [DllImport("shell32.dll")]
        internal static extern int SHQueryUserNotificationState(out QUERY_USER_NOTIFICATION_STATE pquns);
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

        private static bool CheckHorizontalScroll(int x, int y)
        {
            try
            {
                var element = AutomationElement.FromPoint(new System.Windows.Point(x, y));

                if (element == null || element == AutomationElement.RootElement)
                    return false;

                var current = element;

                for (int depth = 0; depth < 12 && current != null; depth++)
                {
                    if (current == AutomationElement.RootElement)
                        break;

                    if (current.TryGetCurrentPattern(ScrollPattern.Pattern, out var patternObj))
                    {
                        var scroll = (ScrollPattern)patternObj;

                        if (scroll.Current.HorizontallyScrollable)
                            return true;
                    }

                    try
                    {
                        current = TreeWalker.ControlViewWalker.GetParent(current);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("CheckHorizontalScroll failed: {Message}", ex.Message);
            }

            return false;
        }

        private static bool IsFullScreenActive()
        {
            try
            {
                if (SHQueryUserNotificationState(out var state) == 0) // S_OK
                {
                    return state == QUERY_USER_NOTIFICATION_STATE.QUNS_BUSY ||
                           state == QUERY_USER_NOTIFICATION_STATE.QUNS_RUNNING_D3D_FULL_SCREEN ||
                           state == QUERY_USER_NOTIFICATION_STATE.QUNS_PRESENTATION_MODE;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("IsFullScreenActive failed: {Message}", ex.Message);
            }

            return false;
        }
        #endregion

        #region Public Methods
        public static void Start()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseUp += GlobalHook_MouseUp;
            _globalHook.MouseDown += GlobalHook_MouseDown;
            _globalHook.MouseMove += GlobalHook_MouseMove;
            _globalHook.MouseDoubleClick += GlobalHook_MouseDoubleClick;
            _globalHook.KeyDown += GlobalHook_KeyDown;
            _globalHook.KeyUp += GlobalHook_KeyUp;

            if (Environment.OSVersion.Version.Build >= 22000)
            {
                if (!TouchInjector.InitializeTouchInjection(feedbackMode: TouchFeedback.NONE))
                {
                    Log.Warning("This system is Windows 11 or later, but touch injection failed!");
                    return;
                }

                IsNativeGestureSupport = true;
            }
            else
            {
                Log.Warning("This system is Windows 10 or lower, native gesture is not supported!");
            }
        }

        public static void Stop()
        {
            if (_globalHook is not null)
            {
                _globalHook.MouseUp -= GlobalHook_MouseUp;
                _globalHook.MouseDown -= GlobalHook_MouseDown;
                _globalHook.MouseMove -= GlobalHook_MouseMove;
                _globalHook.MouseDoubleClick -= GlobalHook_MouseDoubleClick;
                _globalHook.KeyDown -= GlobalHook_KeyDown;
                _globalHook.KeyUp -= GlobalHook_KeyUp;

                _globalHook.Dispose();
                _globalHook = null;
            }
        }
        #endregion

        #region Mouse Events
        private static void GlobalHook_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button is not MouseButtons.Middle)
                return;

            if (_pointers is not null)
            {
                for (int i = 0; i < _pointers.Length; i++)
                {
                    _pointers[i].PointerInfo.PointerFlags = PointerFlags.UP;
                }

                TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
                _pointers = null;
            }
            else if (SettingsManager.Current.UseQuickNewDesktop && _isCtrlDown)
            {
                _inputSimulator.Keyboard
                    .ModifiedKeyStroke(new[] { VirtualKeyCode.LWIN, VirtualKeyCode.CONTROL }, VirtualKeyCode.VK_D);
            }

            _isWheelDown = false;
            _isCanceled = false;
            _hasHorizontalScrollAtClick = false;
            _horizontalScrollCheckTask = null;
        }

        private static void GlobalHook_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button is not MouseButtons.Middle)
                return;

            if (SettingsManager.Current.PauseInFullScreen && IsFullScreenActive())
            {
                _isWheelDown = false;
                return;
            }

            _isWheelDown = true;
            _gestureStartX = e.X;
            _gestureStartY = e.Y;
            _hasHorizontalScrollAtClick = false;

            if (SettingsManager.Current.PrioritizeHorizontalScroll)
            {
                var clickX = e.X;
                var clickY = e.Y;
                _horizontalScrollCheckTask = Task.Run(() =>
                {
                    _hasHorizontalScrollAtClick = CheckHorizontalScroll(clickX, clickY);
                });
            }
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

            if (_pointers is null)
            {
                if (absY >= sensitivity)
                {
                    _isCanceled = true;
                }
                else if (absX >= sensitivity)
                {
                    try { _horizontalScrollCheckTask?.Wait(80); } catch { }

                    if (_hasHorizontalScrollAtClick)
                    {
                        _isCanceled = true;
                        return;
                    }

                    var accelerationMultiplier = SettingsManager.Current.GestureAcceleration switch
                    {
                        0 => 3.0,
                        1 => 1.5,
                        2 => 1.0,
                        _ => 1.5
                    };

                    if (IsNativeGestureSupport)
                    {
                        _pointers = CreatePointers(4).ToArray();

                        for (int i = 0; i < _pointers.Length; i++)
                        {
                            var targetX = e.X;

                            if (SettingsManager.Current.ReverseGestureDirection)
                            {
                                var deltaX = e.X - _gestureStartX;
                                targetX = _gestureStartX - (int)(deltaX * accelerationMultiplier);
                            }
                            else
                            {
                                var deltaX = e.X - _gestureStartX;
                                targetX = _gestureStartX + (int)(deltaX * accelerationMultiplier);
                            }

                            _pointers[i].PointerInfo.PtPixelLocation.X = targetX;
                            _pointers[i].PointerInfo.PtPixelLocation.Y = e.Y;
                            _pointers[i].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
                        }

                        TouchInjector.InjectTouchInput(_pointers.Length, _pointers);
                    }
                    else
                    {
                        var isRightDirection = _gestureStartX - e.X >= 0;

                        if (SettingsManager.Current.ReverseGestureDirection)
                            isRightDirection = !isRightDirection;

                        _inputSimulator.Keyboard
                            .ModifiedKeyStroke(new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.LWIN },
                                isRightDirection
                                    ? VirtualKeyCode.RIGHT
                                    : VirtualKeyCode.LEFT);

                        _isCanceled = true;
                    }
                }
            }
            else
            {
                var accelerationMultiplier = SettingsManager.Current.GestureAcceleration switch
                {
                    0 => 3.0,
                    1 => 1.5,
                    2 => 1.0,
                    _ => 1.5
                };

                for (int i = 0; i < _pointers.Length; i++)
                {
                    var targetX = e.X;

                    if (SettingsManager.Current.ReverseGestureDirection)
                    {
                        var deltaX = e.X - _gestureStartX;
                        targetX = _gestureStartX - (int)(deltaX * accelerationMultiplier);
                    }
                    else
                    {
                        var deltaX = e.X - _gestureStartX;
                        targetX = _gestureStartX + (int)(deltaX * accelerationMultiplier);
                    }

                    _pointers[i].PointerInfo.PtPixelLocation.X = targetX;
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
            if (e.KeyData.HasFlag(Keys.LControlKey) || e.KeyData.HasFlag(Keys.RControlKey))
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
