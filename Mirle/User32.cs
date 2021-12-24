using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mirle
{
    public static class User32
    {
        private static class NativeMethods
        {
            [DllImport("User32.dll")] internal static extern bool SetForegroundWindow(IntPtr hWnd);
            [DllImport("User32.dll")] internal static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        }

        public static void RunningInstanceWithShowNormal(Process instance)
        {
            NativeMethods.ShowWindowAsync(instance.MainWindowHandle, 1);
            NativeMethods.SetForegroundWindow(instance.MainWindowHandle);
        }
        public static void RunningInstanceWithMinImized(Process instance)
        {
            NativeMethods.ShowWindowAsync(instance.MainWindowHandle, 2);
            NativeMethods.SetForegroundWindow(instance.MainWindowHandle);
        }
        public static void RunningInstanceWithMaxImized(Process instance)
        {
            NativeMethods.ShowWindowAsync(instance.MainWindowHandle, 3);
            NativeMethods.SetForegroundWindow(instance.MainWindowHandle);
        }
    }
}
