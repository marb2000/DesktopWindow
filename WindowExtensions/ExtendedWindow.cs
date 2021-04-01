using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using WinRT;
using Microsoft.UI.Xaml.Markup;
using Windows.Foundation;

namespace WinUI.Desktop
{
    public class WindowClosingEventArgs : EventArgs
    {
        public ExtendedWindow Window { get; set; }
        public void TryCancel()
        {
            Window.IsClosing = true;
            Window.Close();
        }
    }

    public class WindowMovingEventArgs : EventArgs
    {
        public ExtendedWindow Window { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
    }

    public class ExtendedWindow : Window
    {
        public enum Placement { Center } //Future, align to the top corner, etc..

        public int Width
        {
            get
            {
                //Get the width
                PInvoke.RECT rc;
                PInvoke.User32.GetWindowRect(_hwnd, out rc);
                int currentWidthInPixels = rc.right - rc.left;

                var dpi = PInvoke.User32.GetDpiForWindow(_hwnd);
                float scalingFactor = (float)dpi / 96;

                return (int)(currentWidthInPixels / scalingFactor);
            }
            set
            {
                SetWindowWidthWin32(_hwnd, value);
            }
        }

        public int Height
        {
            get
            {
                //Get the width
                PInvoke.RECT rc;
                PInvoke.User32.GetWindowRect(_hwnd, out rc);
                int currentHeightInPixels = rc.bottom - rc.top;

                var dpi = PInvoke.User32.GetDpiForWindow(_hwnd);
                float scalingFactor = (float)dpi / 96;

                return (int)(currentHeightInPixels / scalingFactor);

            }
            set
            {
                SetWindowHeightWin32(_hwnd, value);
            }
        }

        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        public bool IsClosing { get; set; }

        public event EventHandler<WindowClosingEventArgs> Closing;
        public event EventHandler<WindowMovingEventArgs> Moving;

        private IntPtr _hwnd = IntPtr.Zero;
        public IntPtr Hwnd
        {
            get { return Hwnd; }
        }

        public ExtendedWindow()
        {
            SubClassingWin32();
        }

        public void SetWindowPlacement(Placement placement)
        {
            switch (placement)
            {
                case Placement.Center:
                    CenterWindowInMonitorWin32(_hwnd);
                    break;
            }
        }

        public void SetWindowPlacement(int top, int left)
        {
            SetWindowPlacementWin32(_hwnd, top, left);
        }

        private void OnClosing()
        {
            WindowClosingEventArgs windowClosingEventArgs = new WindowClosingEventArgs();
            windowClosingEventArgs.Window = this;
            Closing.Invoke(this, windowClosingEventArgs);
        }

        private void OnWindowMoving(int xPos, int yPos)
        {
            WindowMovingEventArgs windowMovingEventArgs = new WindowMovingEventArgs();
            windowMovingEventArgs.Window = this;
            
                       
            windowMovingEventArgs.Top = yPos; //TODO
            windowMovingEventArgs.Left = xPos; //TODO
            Moving.Invoke(this, windowMovingEventArgs);
        }

        private delegate IntPtr WinProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        private WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;
        [DllImport("user32")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);
        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        private void SubClassingWin32()
        {
            //Get the Window's HWND
            _hwnd = this.As<IWindowNative>().WindowHandle;
            if (_hwnd == IntPtr.Zero)
            {
                throw new Exception("The Window Handle is null.");

            }
            newWndProc = new WinProc(NewWindowProc);
            oldWndProc = SetWindowLong(_hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
        }

        private string _iconResource;
        public string Icon
        {
            get { return _iconResource; }
            set
            {
                _iconResource = value;
                LoadIcon(_hwnd, _iconResource);
            }
        }


        private void LoadIcon(IntPtr hwnd, string iconName)
        {

            IntPtr hIcon = PInvoke.User32.LoadImage(IntPtr.Zero, iconName,
                PInvoke.User32.ImageType.IMAGE_ICON, 16, 16, PInvoke.User32.LoadImageFlags.LR_LOADFROMFILE);

            PInvoke.User32.SendMessage(hwnd, PInvoke.User32.WindowMessage.WM_SETICON, (IntPtr)0, hIcon);

        }


        [StructLayout(LayoutKind.Sequential)]
        struct MINMAXINFO
        {
            public PInvoke.POINT ptReserved;
            public PInvoke.POINT ptMaxSize;
            public PInvoke.POINT ptMaxPosition;
            public PInvoke.POINT ptMinTrackSize;
            public PInvoke.POINT ptMaxTrackSize;
        }

        private int ConvertEffectivePixelsToPixels(IntPtr hwnd, int effectivePixels)
        {
            float scalingFactor = GetScalingFactor(hwnd);
            return (int)(effectivePixels * scalingFactor);
        }

        private IntPtr NewWindowProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case PInvoke.User32.WindowMessage.WM_GETMINMAXINFO:
                    //float scalingFactor = GetScalingFactor(hWnd);

                    MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);

                    // Min Width/Height
                    //minMaxInfo.ptMinTrackSize.x = (int)(MinWidth * scalingFactor);
                    //minMaxInfo.ptMinTrackSize.y = (int)(MinHeight * scalingFactor);
                    minMaxInfo.ptMinTrackSize.x = ConvertEffectivePixelsToPixels(hWnd, MinWidth);
                    minMaxInfo.ptMinTrackSize.y = ConvertEffectivePixelsToPixels(hWnd, MinHeight);


                    //Max width/Height
                    //minMaxInfo.ptMaxTrackSize.x = (int)(MaxWidth * scalingFactor);
                    //minMaxInfo.ptMaxTrackSize.y = (int)(MaxHeight * scalingFactor);

                    minMaxInfo.ptMaxTrackSize.x = ConvertEffectivePixelsToPixels(hWnd, MaxWidth);
                    minMaxInfo.ptMaxTrackSize.y = ConvertEffectivePixelsToPixels(hWnd, MaxHeight);

                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;

                case PInvoke.User32.WindowMessage.WM_CLOSE:

                    //Cancel the closing 
                    if (this.Closing is not null)
                    {
                        if (IsClosing == false)
                        {
                            OnClosing();
                        }
                        return IntPtr.Zero;
                    }
                    break;

                case PInvoke.User32.WindowMessage.WM_MOVING:
                    if (this.Moving is not null)
                    {
                        var xPosInPixels = (int)((ushort)lParam & 0xff);  
                        var yPosInPixels = (int)((ushort)lParam >>8);  

                        OnWindowMoving(xPosInPixels, yPosInPixels);

                    }
                    break;

            }
            return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }

        private static float GetScalingFactor(IntPtr hWnd)
        {
            var dpi = PInvoke.User32.GetDpiForWindow(hWnd);
            float scalingFactor = (float)dpi / 96;
            return scalingFactor;
        }

        private void SetWindowSizeWin32(IntPtr hwnd, int width, int height)
        {
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE |
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private void SetWindowWidthWin32(IntPtr hwnd, int width)
        {
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);

            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            int currentHeightInPixels = rc.bottom - rc.top;

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, currentHeightInPixels,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE |
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }
        private void SetWindowHeightWin32(IntPtr hwnd, int height)
        {
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            height = (int)(height * scalingFactor);

            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            int currentWidthInPixels = rc.right - rc.left;

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, currentWidthInPixels, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE |
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }

        private void SetWindowPlacementWin32(IntPtr hwnd, int top, int left)
        {
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;

            int topInPixels = (int)(top * scalingFactor);
            int lefInPixels = (int)(left * scalingFactor);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                lefInPixels, topInPixels, 0, 0,
                PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE |
                PInvoke.User32.SetWindowPosFlags.SWP_NOZORDER |
                PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE);
        }


        private void CenterWindowInMonitorWin32(IntPtr hwnd)
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
        private static void ClipOrCenterRectToMonitorWin32(ref PInvoke.RECT prc, bool UseWorkArea, bool IsCenter)
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
    }
}
