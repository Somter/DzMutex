using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string procc = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            Process[] processes = Process.GetProcessesByName(procc);
            if (processes.Length > 3)
            {
                MessageBox.Show("Приложение не может запустить более 3 копий", 
                                "Ограничение копий", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
            }
        }
    }

}
