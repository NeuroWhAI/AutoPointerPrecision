using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.IO;

namespace AutoPointerPrecision
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public MainWindowVM()
        {
            AddTargetCommand = new CustomCommand(OnAddTarget);
            RemoveTargetCommand = new CustomCommand(OnRemoveTarget);

            LoadTargets();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public CustomCommand AddTargetCommand { get; private set; }
        public CustomCommand RemoveTargetCommand { get; private set; }

        private ObservableCollection<ProcessData> _RunningProcesses = new ObservableCollection<ProcessData>();
        public ObservableCollection<ProcessData> RunningProcesses
        {
            get => _RunningProcesses;
            set
            {
                _RunningProcesses = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ProcessData> TargetProcesses
        { get; private set; } = new ObservableCollection<ProcessData>();

        public async Task UpdateProcessesAsync()
        {
            var processes = await Task.Factory.StartNew(GetProcesses);

            // 이미 타겟에 등록된 프로세스는 제외.
            foreach (var target in TargetProcesses)
            {
                processes.RemoveAll(proc =>
                {
                    if (string.IsNullOrEmpty(target.FileName))
                    {
                        return target.Name == proc.Name;
                    }
                    else
                    {
                        return proc.FileName == target.FileName;
                    }
                });
            }

            RunningProcesses = new ObservableCollection<ProcessData>(processes);
        }

        public async Task CheckTargetProcessesAsync()
        {
            await Task.Yield();


            var foreWin = WinApi.GetForegroundWindow();
            WinApi.GetWindowThreadProcessId(foreWin, out uint foreProcId);

            bool targeted = false;

            foreach (var targetData in TargetProcesses)
            {
                var processes = Process.GetProcessesByName(targetData.Name);

                try
                {
                    foreach (var proc in processes)
                    {
                        if (proc.Id != foreProcId)
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(targetData.FileName)
                            || ProcessUtil.GetFileName(proc) == targetData.FileName)
                        {
                            targeted = true;
                            break;
                        }
                    }

                    if (targeted)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);

                    await Console.Error.WriteLineAsync(e.Message);
                    await Console.Error.WriteLineAsync(e.StackTrace);
                }
                finally
                {
                    foreach (var proc in processes)
                    {
                        proc.Dispose();
                    }
                }
            }

            // Get current settings.
            var mouseParam = new WinApi.MouseParam();
            WinApi.SystemParametersInfo(0x0003, 0, ref mouseParam, 0);

            bool currentOption = (mouseParam.turnOnAccel == 0);

            if (currentOption != targeted)
            {
                // Set new settings.
                mouseParam.turnOnAccel = (targeted ? 0 : 1);
                WinApi.SystemParametersInfo(0x0004, 0, ref mouseParam, 2);
            }
        }

        private void OnAddTarget(object param)
        {
            if (param is ProcessData data)
            {
                RunningProcesses.Remove(data);
                TargetProcesses.Add(data);
                
                SaveTargets();
            }
        }

        private void OnRemoveTarget(object param)
        {
            if (param is ProcessData data)
            {
                TargetProcesses.Remove(data);

                SaveTargets();
            }
        }

        private List<ProcessData> GetProcesses()
        {
            var exclusionNames = new[]
            {
                "svchost", "csrss", "smss", "wininit", "lsass", "ntoskrnl", "sihost",
                "ntoskrnl", "winlogon", "dwm", "services", "conhost", "taskhostw",
                "runtimebroker", "spoolsv", "ctfmon",
            };


            var result = new List<ProcessData>();

            var processes = new Dictionary<string, Process>();

            foreach (var proc in Process.GetProcesses())
            {
                string name = proc.ProcessName.ToLower();

                if (exclusionNames.Any(filter => filter == name))
                {
                    proc.Dispose();
                }
                else
                {
                    string filename = ProcessUtil.GetFileName(proc);

                    if (string.IsNullOrEmpty(filename))
                    {
                        var data = new ProcessData(proc);

                        if (!string.IsNullOrEmpty(data.Name))
                        {
                            result.Add(data);
                        }
                    }
                    else if (processes.ContainsKey(filename))
                    {
                        proc.Dispose();
                    }
                    else
                    {
                        processes.Add(filename, proc);
                    }
                }
            }

            foreach (var kv in processes)
            {
                var data = new ProcessData(kv.Value);

                if (!string.IsNullOrEmpty(data.Name))
                {
                    result.Add(data);
                }
            }

            result.Sort((left, right) => left.Name.CompareTo(right.Name));


            return result;
        }

        private void SaveTargets()
        {
            using (var sw = new StreamWriter(new FileStream("save.txt", FileMode.Create)))
            {
                sw.WriteLine(TargetProcesses.Count);

                foreach (var data in TargetProcesses)
                {
                    sw.WriteLine(data.Name);
                    sw.WriteLine(data.FileName);
                }

                sw.Close();
            }
        }

        private void LoadTargets()
        {
            if (!File.Exists("save.txt"))
            {
                return;
            }

            var processes = new List<ProcessData>();

            using (var sr = new StreamReader(new FileStream("save.txt", FileMode.Open)))
            {
                if (int.TryParse(sr.ReadLine(), out int cnt))
                {
                    for (int i = 0; i < cnt; ++i)
                    {
                        string name = sr.ReadLine();
                        string filename = sr.ReadLine();

                        processes.Add(new ProcessData(name, filename));
                    }
                }

                sr.Close();
            }

            TargetProcesses = new ObservableCollection<ProcessData>(processes);
        }
    }
}
