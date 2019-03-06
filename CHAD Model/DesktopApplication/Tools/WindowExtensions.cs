using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopApplication.Tools
{
    public static class WindowExtensions
    {
        #region Static Fields and Constants

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_DLGMODALFRAME = 0x0001;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int SWP_FRAMECHANGED = 0x0020;
        private const uint WM_SETICON = 0x0080;

        #endregion

        #region All other members

        public static void RemoveIcon(this Window window)
        {
            if (null == window) return;

            var hWnd = new WindowInteropHelper(window).Handle;

            var exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_DLGMODALFRAME);

            SendMessage(hWnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
            SendMessage(hWnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion
    }
}