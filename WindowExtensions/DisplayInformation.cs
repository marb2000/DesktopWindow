﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WinUIExtensions.Desktop
{

    public class DisplayInfo
    {
        public string Availability { get; set; }
        public int ScreenHeight { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenEfectiveHeight
        {
            get
            {
                int widthDPI;
                _ = PInvoke.SHCore.GetDpiForMonitor(hMonitor,
                    PInvoke.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out widthDPI, out _);
                float scalingFactor = (float)widthDPI / 96;
                return (int)(ScreenHeight / scalingFactor);
            }
        }
        public int ScreenEfectiveWidth
        {
            get
            {
                int heightDPI;
                _ = PInvoke.SHCore.GetDpiForMonitor(hMonitor,
                    PInvoke.MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out _, out heightDPI);
                float scalingFactor = (float)heightDPI / 96;
                return (int)(ScreenWidth / scalingFactor);
            }
        }
        public string DeviceName { get; set; }
        public PInvoke.RECT WorkArea { get; set; }
        public IntPtr hMonitor { get; set; }
    }

    unsafe public class DisplayInformation
    {
        public static int ConvertEpxToPixel(IntPtr hwnd, int effectivePixels)
        {
            float scalingFactor = GetScalingFactor(hwnd);
            return (int)(effectivePixels * scalingFactor);
        }

        public static int ConvertPixelToEpx(IntPtr hwnd, int pixels)
        {
            float scalingFactor = GetScalingFactor(hwnd);
            return (int)(pixels / scalingFactor);
        }
        
        public static float GetScalingFactor(IntPtr hwnd)
        {
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            return scalingFactor;
        }

        public static DisplayInfo GetDisplay(IntPtr hwnd)
        {
            DisplayInfo di = null;
            IntPtr hMonitor;
            PInvoke.RECT rc;
            PInvoke.User32.GetWindowRect(hwnd, out rc);
            hMonitor = PInvoke.User32.MonitorFromRect(ref rc, PInvoke.User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);

            PInvoke.User32.MONITORINFOEX mi = new PInvoke.User32.MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf(mi);
            bool success = PInvoke.User32.GetMonitorInfo(hMonitor, out mi);
            if (success)
            {
                di = ConvertMonitorInfoToDisplayInfo(hMonitor, mi);
            }
            return di;
        }

        private static DisplayInfo ConvertMonitorInfoToDisplayInfo(IntPtr hMonitor, PInvoke.User32.MONITORINFOEX mi)
        {
            return new DisplayInfo
            {
                ScreenWidth = mi.Monitor.right - mi.Monitor.left,
                ScreenHeight = mi.Monitor.bottom - mi.Monitor.top,
                DeviceName = new string(mi.DeviceName),
                WorkArea = mi.WorkArea,
                Availability = mi.Flags.ToString(),
                hMonitor = hMonitor
            };
        }

        unsafe public static List<DisplayInfo> GetDisplays()
        {
            List<DisplayInfo> col = new();

            _ = EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref PInvoke.RECT lprcMonitor, IntPtr dwData)
                {
                    PInvoke.User32.MONITORINFOEX mi = new PInvoke.User32.MONITORINFOEX();
                    mi.cbSize = Marshal.SizeOf(mi);
                    bool success = PInvoke.User32.GetMonitorInfo(hMonitor, out mi);
                    if (success)
                    {
                        DisplayInfo di = ConvertMonitorInfoToDisplayInfo(hMonitor, mi);
                        col.Add(di);
                    }
                    return true;
                }, IntPtr.Zero);
            return col;
        }

        public enum UserInteractionModeEnum { Touch, Mouse };
        public static UserInteractionModeEnum UserInteractionMode
        {
            get
            {
                // TODO: Have a counterpart event listeining the message WM_SETTINGCHANGE
                UserInteractionModeEnum userInteractionMode = UserInteractionModeEnum.Mouse;
                int SM_CONVERTIBLESLATEMODE = 0x2003;
                int state = GetSystemMetrics(SM_CONVERTIBLESLATEMODE);//O for tablet
                if(state == 0)
                {
                    userInteractionMode = UserInteractionModeEnum.Touch;
                }
                return userInteractionMode;
            }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetSystemMetrics")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);
        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref PInvoke.RECT lprcMonitor, IntPtr dwData);

    }
}
