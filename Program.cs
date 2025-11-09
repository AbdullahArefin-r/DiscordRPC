using System;
using System.Threading;
using System.Windows.Forms;

namespace DiscordActivityMonitor
{
    static class Program
    {
        private static Mutex? mutex = null;
        
        [STAThread]
        static void Main()
        {
            const string mutexName = "DiscordActivityMonitor_SingleInstance_Mutex";
            bool createdNew;
            
            mutex = new Mutex(true, mutexName, out createdNew);
            
            if (!createdNew)
            {
                // Another instance is already running
                MessageBox.Show(
                    "Discord Activity Monitor is already running!\n\n" +
                    "Check your system tray (bottom right corner) for the app icon.",
                    "Already Running",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            
            // Release mutex when app closes
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
