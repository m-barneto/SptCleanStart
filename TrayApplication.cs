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
        private Process ServerProc;
        private Process LauncherProc;

        public TrayApplication() {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon() {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            // Start server
            if (!File.Exists("SPT.Server.exe")) {
                MessageBox.Show("Unable to locate server executable!");
                Application.Exit();
            }
            if (!File.Exists("SPT.Launcher.exe")) {
                MessageBox.Show("Unable to locate server executable!");
                Application.Exit();
            }

            ProcessStartInfo startInfo = new ProcessStartInfo("SPT.Server.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ServerProc = Process.Start(startInfo);

            LauncherProc = Process.Start("SPT.Launcher.exe");

            heartbeatTimer = new DispatcherTimer();
            heartbeatTimer.Tick += new EventHandler(heartbeatTimer_Tick);
            heartbeatTimer.Interval = TimeSpan.FromSeconds(5);
            heartbeatTimer.Start();
        }

        bool IsClientVisible() {
            return Process.GetProcessesByName("EscapeFromTarkov.exe").Length > 0;
        }

        private async void heartbeatTimer_Tick(object sender, EventArgs e) {
            if (hasClientStarted) {
                // If we have a client and the launcher is still alive
                if (LauncherProc != null) {
                    // Kill the launcher!
                    LauncherProc.Kill();
                    LauncherProc.Close();
                    LauncherProc = null;
                }

                // If we had a client and it's no longer visible
                if (!IsClientVisible()) {
                    // Kill the server! (after letting it save) :)
                    await Task.Delay(5000);
                    // sleep for a bit of time so that the server can have a chance to save
                    // might need to be profileSaveIntervalSeconds long to make sure?
                    Console.WriteLine("WE GONNA KILL OURSELVES RAAAAAAAAAA");
                    Exit(null, null);
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
