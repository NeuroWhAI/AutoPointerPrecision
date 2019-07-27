using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoPointerPrecision
{
    public class CustomCommand : ICommand
    {
        public CustomCommand(Action<object> job, bool firstEnabled = true)
        {
            Job = job;
            _IsEnabled = firstEnabled;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public void Execute(object parameter)
        {
            Job?.Invoke(parameter);
        }

        public Action<object> Job { get; set; }

        private bool _IsEnabled;
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (IsEnabled != value)
                {
                    _IsEnabled = value;

                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
