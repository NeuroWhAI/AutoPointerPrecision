using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AutoPointerPrecision
{
    using NotifyIcon = System.Windows.Forms.NotifyIcon;

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            m_tray.Icon = Properties.Resources.Icon;
            m_tray.MouseDoubleClick += Tray_MouseDoubleClick;
            m_tray.Text = this.Title;

            m_vm = this.DataContext as MainWindowVM;

            m_tmrUpdate.Interval = TimeSpan.FromSeconds(3.0);
            m_tmrUpdate.Tick += TmrUpdate_Tick;

            m_tmrCheck.Interval = TimeSpan.FromSeconds(1.0);
            m_tmrCheck.Tick += TmrCheck_Tick;
        }

        private NotifyIcon m_tray = new NotifyIcon();
        private MainWindowVM m_vm = null;
        private DispatcherTimer m_tmrUpdate = new DispatcherTimer();
        private DispatcherTimer m_tmrCheck = new DispatcherTimer();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            m_tray.Visible = true;
            m_tmrUpdate.Interval = TimeSpan.FromSeconds(60.0);

            m_tmrUpdate.Start();
            m_tmrCheck.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!m_tray.Visible)
            {
                this.Hide();
                m_tray.Visible = true;
                m_tmrUpdate.Interval = TimeSpan.FromSeconds(60.0);

                e.Cancel = true;
            }
        }

        private void Tray_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                m_tmrUpdate.Interval = TimeSpan.FromSeconds(3.0);
                m_tray.Visible = false;
                this.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private async void TmrUpdate_Tick(object sender, EventArgs e)
        {
            m_tmrUpdate.Stop();

            try
            {
                await m_vm.UpdateProcessesAsync();
            }
            finally
            {
                m_tmrUpdate.Start();
            }
        }

        private async void TmrCheck_Tick(object sender, EventArgs e)
        {
            m_tmrCheck.Stop();

            try
            {
                await m_vm.CheckTargetProcessesAsync();
            }
            finally
            {
                m_tmrCheck.Start();
            }
        }
    }
}
