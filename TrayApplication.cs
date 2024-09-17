using SptCleanStart.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SptCleanStart {
    internal class TrayApplication : ApplicationContext {
        DispatcherTimer heartbeatTimer;
        bool hasClientStarted = false;

        private NotifyIcon trayIcon;
        private IntPtr ServerHandle;
        private Process ServerProc;

        public TrayApplication() {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon() {
                    Icon = Resources.AppIcon,
                    ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit),
                }),
                Visible = true
            };

            // Start server
            if (!File.Exists("SPT.Server.exe")) {
                MessageBox.Show("Unable to locate server executable!");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo("SPT.Server.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ServerProc = Process.Start(startInfo);

            heartbeatTimer = new DispatcherTimer();
            heartbeatTimer.Tick += new EventHandler(heartbeatTimer_Tick);
            heartbeatTimer.Interval = new TimeSpan(0, 0, 1);
            heartbeatTimer.Start();
        }

        bool IsClientVisible() {
            return Process.GetProcessesByName("EscapeFromTarkov.exe").Length > 0;
        }

        private void heartbeatTimer_Tick(object sender, EventArgs e) {
            if (hasClientStarted) {
                if (!IsClientVisible()) {
                    Console.WriteLine("WE GONNA KILL IT RAAAAAAAAAA");
                }
            } else {
                // Client hasn't started yet, lets just wait and see if it does start
                hasClientStarted = IsClientVisible();
            }
        }

        void Exit(object sender, EventArgs e) {
            ServerProc.Kill();
            ServerProc.Close();
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}
