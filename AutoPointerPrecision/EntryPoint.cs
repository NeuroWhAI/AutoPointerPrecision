using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using Microsoft.Win32;

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


                if (UpdateManager.CheckUpdate())
                {
                    if (MessageBox.Show("업데이트가 있습니다.\n다운로드 페이지를 띄우고 종료하시겠습니까?", "Auto Pointer Precision",
                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        Process.Start("https://neurowhai.tistory.com/361");
                        return;
                    }
                }


#if !DEBUG
                try
                {
                    // Register as a startup program.
                    using (var reg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        reg.SetValue("AutoPointerPrecision_NeuroWhAI", Assembly.GetExecutingAssembly().Location);
                    }
                }
                catch (SecurityException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.Error.WriteLine(e.Message);
                }
#endif


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
