using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace AutoPointerPrecision
{
    using Icon = System.Drawing.Icon;

    public class ProcessData : IDisposable
    {
        ~ProcessData()
        {
            Dispose(false);
        }

        public ProcessData(string name, string filename)
        {
            FileName = filename;
            Name = name;
            Icon = ProcessUtil.GetIconFromPath(filename);
            DisplayName = MakeDisplayName(name, filename);
        }

        public ProcessData(Process proc)
        {
            try
            {
                FileName = ProcessUtil.GetFileName(proc);
                Icon = ProcessUtil.GetIconFromPath(FileName);
                Name = proc.ProcessName;
                DisplayName = MakeDisplayName(Name, FileName);
            }
            catch (InvalidOperationException)
            {
                FileName = string.Empty;
                Icon = null;
                Name = string.Empty;
                DisplayName = string.Empty;
            }
        }

        public Icon Icon { get; private set; }
        public string Name { get; private set; }
        public string FileName { get; private set; }
        public string DisplayName { get; private set; }

        public bool Disposed { get; private set; } = false;

        private string MakeDisplayName(string name, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return name;
            }
            else
            {
                string path = Path.GetDirectoryName(filename);
                int subIndex = path.LastIndexOf('\\');

                if (subIndex >= 0)
                {
                    path = path.Substring(subIndex + 1) + '\\';
                }

                return $"{name} ({path})";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                // Release managed resources.
            }

            // Release unmanaged resources.
            Icon?.Dispose();

            Disposed = true;
        }
    }
}
