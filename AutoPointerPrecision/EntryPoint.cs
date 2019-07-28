using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace AutoPointerPrecision
{
    class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (TaskCanceledException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
            catch (Exception e)
            {
                MessageBox.Show($@"There was a problem that could not be handled.
처리하지 못한 문제가 발생하였습니다.
Please capture the contents below.
아래 내용을 캡쳐하여 보내주세요.

{e.Message}
{e.StackTrace}", "Error");
            }
        }
    }
}
