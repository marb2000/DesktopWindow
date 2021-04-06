using Microsoft.UI.Xaml;

using System;
using System.Runtime.InteropServices;

using WinRT;

namespace WinUIExtensions.Desktop
{

    public class WindowPosition
    {
        public int Top { get; private set; }
        public int Left { get; private set; }
        public WindowPosition(int top, int left)
        {
            this.Top = top;
            this.Left = left;
        }
    }

    public class WindowSizingEventArgs : EventArgs
    {
        public DesktopWindow Window { get; private set; }
        public WindowSizingEventArgs(DesktopWindow window)
        {
            Window = window;
        }
    }

    public class WindowClosingEventArgs : EventArgs
    {
        public DesktopWindow Window { get; private set; }
        public WindowClosingEventArgs(DesktopWindow window)
        {
            Window = window;
        }

        public void TryCancel()
        {
            Window.IsClosing = true;
            Window.Close();
        }
    }

    public class WindowDpiChangedEventArgs : EventArgs
    {
        public DesktopWindow Window { get; private set; }
        public int Dpi{ get; private set; }
        
        public WindowDpiChangedEventArgs(DesktopWindow window, int newDpi)
        {
            Window = window;
            Dpi = newDpi;
        }
    }

    public class WindowOrientationChangedEventArgs : EventArgs
    {
        public DesktopWindow Window { get; private set; }
        public DesktopWindow.Orientation Orientation { get; private set; }

        public WindowOrientationChangedEventArgs(DesktopWindow window, DesktopWindow.Orientation newOrientationi)
        {
            Window = window;
            Orientation = newOrientationi;
        }
    }

    public class WindowMovingEventArgs : EventArgs
    {
        public DesktopWindow Window { get; private set; }
        public WindowPosition NewPosition { get; private set; }
        public int Top { get; private set; }
        public int Left { get; private set; }
        public WindowMovingEventArgs(DesktopWindow window, WindowPosition windowPosition)
        {
            Window = window;
            NewPosition = new(windowPosition.Top, windowPosition.Left);
        }
    }

    public class DesktopWindow : Window
    {
        public enum Orientation { Landscape, Portrait }

        public enum Placement { Center, TopLeftCorner, BottomLeftCorner } //Future: align to the top corner, etc..

        public int Width
        {
            get
            {
                return DisplayInformation.ConvertPixelToEpx(_hwnd, GetWidthWin32(_hwnd));
            }
            set
            {
                SetWindowWidthWin32(_hwnd, DisplayInformation.ConvertEpxToPixel(_hwnd, value));
            }
        }

        public int Height
        {
            get
            {
                return DisplayInformation.ConvertPixelToEpx(_hwnd,GetHeightWin32(_hwnd));
            }
            set
            {
                SetWindowHeightWin32(_hwnd, DisplayInformation.ConvertEpxToPixel(_hwnd, value));
            }
        }
        
        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        public bool IsClosing { get; set; }

        public event EventHandler<WindowClosingEventArgs> Closing;
        public event EventHandler<WindowMovingEventArgs> Moving;
        public event EventHandler<WindowSizingEventArgs> Sizing;
        public event EventHandler<WindowDpiChangedEventArgs> DpiChanged;
        public event EventHandler<WindowOrientationChangedEventArgs> OrientationChanged;

        public IntPtr Hwnd
        {
            get { return _hwnd; }
        }

        public int Dpi
        {
            get { return PInvoke.User32.GetDpiForWindow(_hwnd); }
        }


        public DesktopWindow()
        {
            SubClassingWin32();
            _currentOrientation = GetWindowOrientationWin32(_hwnd);
        }

        public void SetWindowPlacement(Placement placement)
        {
            switch (placement)
            {
                case Placement.Center:
                    PlacementCenterWindowInMonitorWin32(_hwnd);
                    break;
                case Placement.TopLeftCorner:
                    PlacementTopLefWindowInMonitorWin32(_hwnd);
                    break;
                case Placement.BottomLeftCorner:
                    PlacementBottomLefWindowInMonitorWin32(_hwnd);
                    break;
            }
        }

        public void SetWindowPlacement(int topExp, int leftExp)
        {
            SetWindowPlacementWin32(_hwnd, DisplayInformation.ConvertEpxToPixel(_hwnd, topExp),
                                           DisplayInformation.ConvertEpxToPixel(_hwnd, leftExp));
        }

        public WindowPosition GetWindowPosition()
        {
            //windowPosition comes in pixels(Win32), so you need to convert into epx
            WindowPosition windowPosition = GetWindowPositionWin32(_hwnd);

            return new(DisplayInformation.ConvertPixelToEpx(_hwnd, windowPosition.Top),
                       DisplayInformation.ConvertPixelToEpx(_hwnd, windowPosition.Left));
        }
                
        public string Icon
        {
            get { return _iconResource; }
            set
            {
                _iconResource = value;
                LoadIcon(_hwnd, _iconResource);
            }
        }

        public void Maximize()
        {
            _ = PInvoke.User32.ShowWindow(_hwnd, PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
        }

        public void Minimize()
        {
            _ = PInvoke.User32.ShowWindow(_hwnd, PInvoke.User32.WindowShowStyle.SW_MINIMIZE);
        }

        public void Restore()
        {
            _ = PInvoke.User32.ShowWindow(_hwnd, PInvoke.User32.WindowShowStyle.SW_RESTORE);
        }

        public void Hide()
        {
            _ = PInvoke.User32.ShowWindow(_hwnd, PInvoke.User32.WindowShowStyle.SW_HIDE);
        }

        public void BringToTop()
        {
            _ = PInvoke.User32.SetWindowPos(_hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOPMOST, 0, 0, 0, 0,
                PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE | PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE);
        }


        #region Private
        private string _iconResource;
        private IntPtr _hwnd = IntPtr.Zero;
        Orientation _currentOrientation;

        private void OnClosing()
        {
            WindowClosingEventArgs windowClosingEventArgs = new(this);
            Closing.Invoke(this, windowClosingEventArgs);
        }

        private void OnWindowMoving()
        {
            var windowPosition = GetWindowPositionWin32(_hwnd);
            //windowPosition comes in pixels(Win32), so you need to convert into epx
            WindowMovingEventArgs windowMovingEventArgs = new(this, 
                new WindowPosition(
                    DisplayInformation.ConvertPixelToEpx(_hwnd, windowPosition.Top),
                    DisplayInformation.ConvertPixelToEpx(_hwnd, windowPosition.Left)));
            Moving.Invoke(this, windowMovingEventArgs);
        }
        private void OnWindowSizing()
        {
            WindowSizingEventArgs windowSizingEventArgs = new(this);
            Sizing.Invoke(this, windowSizingEventArgs);
        }

        private void OnWindowDpiChanged(int newDpi)
        {
            WindowDpiChangedEventArgs windowDpiChangedEvent = new(this, newDpi);
            DpiChanged.Invoke(this, windowDpiChangedEvent);
        }

        private void OnWindowOrientationChanged(Orientation newOrinetation)
        {
            WindowOrientationChangedEventArgs windowOrientationChangedEventArgs = new(this, newOrinetation);
            OrientationChanged.Invoke(this, windowOrientationChangedEventArgs);
        }

        private delegate IntPtr WinProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        private WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;
        [DllImport("user32")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);
        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        private void SubClassingWin32()
        {
            //Get the Window's HWND
            _hwnd = this.As<IWindowNative>().WindowHandle;
            if (_hwnd == IntPtr.Zero)
            {
                throw new NullReferenceException("The Window Handle is null.");

            }
            newWndProc = new WinProc(NewWindowProc);
            oldWndProc = SetWindowLongPtr(_hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
        }

        private void LoadIcon(IntPtr hwnd, string iconName)
        {

            IntPtr hIcon = PInvoke.User32.LoadImage(IntPtr.Zero, iconName,
                PInvoke.User32.ImageType.IMAGE_ICON, 16, 16, PInvoke.User32.LoadImageFlags.LR_LOADFROMFILE);

            PInvoke.User32.SendMessage(hwnd, PInvoke.User32.WindowMessage.WM_SETICON, (IntPtr)0, hIcon);

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public PInvoke.POINT ptReserved;
            public PInvoke.POINT ptMaxSize;
            public PInvoke.POINT ptMaxPosition;
            public PInvoke.POINT ptMinTrackSize;
            public PInvoke.POINT ptMaxTrackSize;
        }
        
        private IntPtr NewWindowProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case PInvoke.User32.WindowMessage.WM_GETMINMAXINFO:
                    MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.x = DisplayInformation.ConvertEpxToPixel(hWnd, MinWidth);
                    minMaxInfo.ptMinTrackSize.y = DisplayInformation.ConvertEpxToPixel(hWnd, MinHeight);
                    minMaxInfo.ptMaxTrackSize.x = DisplayInformation.ConvertEpxToPixel(hWnd, MaxWidth);
                    minMaxInfo.ptMaxTrackSize.y = DisplayInformation.ConvertEpxToPixel(hWnd, MaxHeight);
                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;

                case PInvoke.User32.WindowMessage.WM_CLOSE:

                    //If there is a Closing event handler and the close message wasn't send via
                    //this event (that set IsClosing=true), the message is ignored. 
                    if (this.Closing is not null)
                    {
                        if (IsClosing == false)
                        {
                            OnClosing();
                        }
                        return IntPtr.Zero;
                    }
                    break;

                case PInvoke.User32.WindowMessage.WM_MOVE:
                    if (this.Moving is not null)
                    {
                        OnWindowMoving();
                    }
                    break;
                case PInvoke.User32.WindowMessage.WM_SIZING:
                    if (this.Sizing is not null)
                    {
                        OnWindowSizing();
                    }
                    break;
                case PInvoke.User32.WindowMessage.WM_DPICHANGED:
                    if(this.DpiChanged is not null)
                    {
                        //g_dpi = HIWORD(wParam);
                        uint dpi = HiWord(wParam);
                        OnWindowDpiChanged((int)dpi);
                    }
                    break;
                case PInvoke.User32.WindowMessage.WM_DISPLAYCHANGE:
                    if (this.OrientationChanged is not null)
                    {
                        var newOrinetation = GetWindowOrientationWin32(hWnd);
                        if (newOrinetation != _currentOrientation)
                        {
                            _currentOrientation = newOrinetation;
                            OnWindowOrientationChanged(newOrinetation);
                        }
                    }
                    break;
            }
            return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }

        private Orientation GetWindowOrientationWin32(IntPtr hwnd)
        {
            Orientation orientationEnum; 
            int theScreenWidth = DisplayInformation.GetDisplay(hwnd).ScreenWidth;
            int theScreenHeight = DisplayInformation.GetDisplay(hwnd).ScreenHeight;
            if (theScreenWidth > theScreenHeight)
                orientationEnum = Orientation.Landscape;
            else
                orientationEnum = Orientation.Portrait;
            return orientationEnum;
        }

        private static uint HiWord(IntPtr ptr)
        {
            uint value = (uint)(int)ptr;
            if ((value & 0x80000000) == 0x80000000)
                return (value >> 16);
            else
                return (value >> 16) & 0xffff;
        }

        private int GetWidthWin32(IntPtr hwnd)
        {
            //Get the width
            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            return rc.right - rc.left;
        }

        private int GetHeightWin32(IntPtr hwnd)
        {
            //Get the width
            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            return rc.bottom - rc.top;
        }

        private void SetWindowSizeWin32(IntPtr hwnd, int width, int height)
        {
            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE |
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private WindowPosition GetWindowPositionWin32(IntPtr hwnd)
        {
            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            return new WindowPosition(rc.top, rc.left);
        }

        private void SetWindowWidthWin32(IntPtr hwnd, int width)
        {
            int currentHeightInPixels = GetHeightWin32(hwnd);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, currentHeightInPixels,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE |
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private void SetWindowHeightWin32(IntPtr hwnd, int height)
        {
            int currentWidthInPixels = GetWidthWin32(hwnd);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, currentWidthInPixels, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE |
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private void PlacementTopLefWindowInMonitorWin32(IntPtr hwnd)
        {
            var displayInfo = DisplayInformation.GetDisplay(hwnd);
            SetWindowPlacementWin32(hwnd, displayInfo.WorkArea.top, displayInfo.WorkArea.left);
        }
        private void PlacementBottomLefWindowInMonitorWin32(IntPtr hwnd)
        {
            var displayInfo = DisplayInformation.GetDisplay(hwnd);
            SetWindowPlacementWin32(hwnd, displayInfo.WorkArea.bottom - GetHeightWin32(_hwnd), displayInfo.WorkArea.left);
        }


        private void SetWindowPlacementWin32(IntPtr hwnd, int top, int left)
        {
            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                left, top, 0, 0,
                PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE |
                PInvoke.User32.SetWindowPosFlags.SWP_NOZORDER |
                PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private void PlacementCenterWindowInMonitorWin32(IntPtr hwnd)
        {
            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            ClipOrCenterRectToMonitorWin32(ref rc, true, true);
            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                rc.left, rc.top, 0, 0,
                PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE |
                PInvoke.User32.SetWindowPosFlags.SWP_NOZORDER |
                PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private void ClipOrCenterRectToMonitorWin32(ref PInvoke.RECT prc, bool UseWorkArea, bool IsCenter)
        {
            IntPtr hMonitor;
            PInvoke.RECT rc;
            int w = prc.right - prc.left;
            int h = prc.bottom - prc.top;

            hMonitor = PInvoke.User32.MonitorFromRect(ref prc, PInvoke.User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            PInvoke.User32.MONITORINFO mi = new PInvoke.User32.MONITORINFO();
            mi.cbSize = Marshal.SizeOf(mi);

            PInvoke.User32.GetMonitorInfo(hMonitor, ref mi);

            rc = UseWorkArea ? mi.rcWork : mi.rcMonitor;

            if (IsCenter)
            {
                prc.left = rc.left + (rc.right - rc.left - w) / 2;
                prc.top = rc.top + (rc.bottom - rc.top - h) / 2;
                prc.right = prc.left + w;
                prc.bottom = prc.top + h;
            }
            else
            {
                prc.left = Math.Max(rc.left, Math.Min(rc.right - w, prc.left));
                prc.top = Math.Max(rc.top, Math.Min(rc.bottom - h, prc.top));
                prc.right = prc.left + w;
                prc.bottom = prc.top + h;
            }
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
        #endregion
    }
}
