using System;
using System.Diagnostics;
using System.Reflection;

namespace Mirle
{
    public static class AppEnvironment
    {
        public static bool IsDuplicatedRunningInstance()
        {
            var current = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(current.ProcessName);
            foreach (var process in processes)
            {
                if (process.MainModule.FileName == current.MainModule.FileName && process.Id != current.Id)
                {
                    return true;
                }
            }
            return false;
        }
        public static Process GetDuplicatedRunningInstance()
        {
            var current = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(current.ProcessName);
            if (processes.Length > 1)
            {
                foreach (var process in processes)
                {
                    if (process.MainModule.FileName == current.MainModule.FileName && process.Id != current.Id)
                    {
                        return process;
                    }
                }
            }
            return null;
        }

        public static string GetProductName()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                object[] objects = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                var apa = (AssemblyProductAttribute)objects[0];
                return apa.Product;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            return string.Empty;
        }
    }
}
