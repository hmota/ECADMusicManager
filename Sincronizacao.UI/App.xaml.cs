using System.Diagnostics;
using System.Windows;

namespace SincronizacaoMusical.UI
{
    /// <summary>
    /// Inicio da Aplicação
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Inicializa a aplicação
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                MessageBox.Show("O aplicativo já está com um processo aberto.");
                Current.Shutdown();
            }
        }
    }
}
