using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Management;
using System.Drawing;
using System.IO;

namespace AutoPointerPrecision
{
    public static class ProcessUtil
    {
        public static string GetFileName(Process proc)
        {
            try
            {
                return proc.MainModule.FileName;
            }
            catch (Win32Exception)
            {
                return QueryFileName(proc);
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        public static string QueryFileName(Process proc)
        {
            string query = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + proc.Id;
            using (var searcher = new ManagementObjectSearcher(query))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }

            return string.Empty;
        }

        public static Icon GetIcon(Process proc)
        {
            return GetIconFromPath(GetFileName(proc));
        }

        public static Icon GetIconFromPath(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                try
                {
                    return Icon.ExtractAssociatedIcon(path);
                }
                catch (IOException)
                {
                    return null;
                }
            }

            return null;
        }
    }
}
