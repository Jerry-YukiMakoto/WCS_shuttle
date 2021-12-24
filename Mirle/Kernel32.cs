using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Mirle
{
    public static class Kernel32
    {
        private static class NativeMethods
        {
            [DllImport("Kernel32.dll")]
            internal static extern bool SetLocalTime(ref SYSTEMTIME sysTime);
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern int WritePrivateProfileString(string section, string key, string value, string filePath);
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            internal static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnValue, int size, string filePath);
        }

        public static void SetLocalDatetime(DateTime newDateTime)
        {
            try
            {
                var st = new SYSTEMTIME();
                st.wYear = Convert.ToUInt16(newDateTime.Year);
                st.wMonth = Convert.ToUInt16(newDateTime.Month);
                st.wDay = Convert.ToUInt16(newDateTime.Day);
                st.wHour = Convert.ToUInt16(newDateTime.Hour);
                st.wMilliseconds = Convert.ToUInt16(newDateTime.Millisecond);
                st.wMinute = Convert.ToUInt16(newDateTime.Minute);
                st.wSecond = Convert.ToUInt16(newDateTime.Second);
                NativeMethods.SetLocalTime(ref st);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }

        private struct SYSTEMTIME
        {
            public ushort wDay;
            public ushort wDayOfWeek;
            public ushort wHour;
            public ushort wMilliseconds;
            public ushort wMinute;
            public ushort wMonth;
            public ushort wSecond;
            public ushort wYear;

            public void SubIni()
            {
                wYear = 0;
                wMonth = 0;
                wDayOfWeek = 0;
                wDay = 0;
                wHour = 0;
                wMinute = 0;
                wSecond = 0;
                wMilliseconds = 0;
            }
        }
    }
}
