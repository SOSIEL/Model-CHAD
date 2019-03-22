using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CHAD.DesktopApplication.Tools
{
    public static class WindowExtensions
    {
        #region Static Fields and Constants

        private const int GwlExStyle = -20;
        private const int WsExDlgModalFrame = 0x0001;
        private const int SwpNoSize = 0x0001;
        private const int SwpNoMove = 0x0002;
        private const int SwpNoZOrder = 0x0004;
        private const int SwpNoActivate = 0x0010;
        private const int SwpFrameChanged = 0x0020;
        private const uint WmSetIcon = 0x0080;

        #endregion

        #region All other members

        public static void RemoveIcon(this Window window)
        {
            if (null == window) return;

            var hWnd = new WindowInteropHelper(window).Handle;

            var exStyle = GetWindowLong(hWnd, GwlExStyle);
            SetWindowLong(hWnd, GwlExStyle, exStyle | WsExDlgModalFrame);

            SendMessage(hWnd, WmSetIcon, IntPtr.Zero, IntPtr.Zero);
            SendMessage(hWnd, WmSetIcon, new IntPtr(1), IntPtr.Zero);

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SwpNoMove | SwpNoSize | SwpNoZOrder | SwpNoActivate | SwpFrameChanged);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion
    }
}