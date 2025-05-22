using FakeUpdate.Forms;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsDisplayAPI;

namespace FakeUpdate
{
    internal static class Program
    {
        private static int explorerPid = -1;
        
        private const int SPI_SETSCREENSAVERRUNNING = 0x0061;
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DisableTaskManager(true);

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            
            KillExplorer();

            Application.ApplicationExit += (s, e) => 
            {
                RestoreExplorer();
                DisableTaskManager(false);
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            };

            var displays = Display.GetDisplays().Where(d => d.IsValid).ToList();

            var virtualDisplays = displays
                .Where(d => d.Adapter?.DeviceName == "USB Mobile Monitor Virtual Display")
                .ToList();

            if (virtualDisplays.Count == 0)
            {
                // no virtual monitor  
                return;
            }

            if (displays.Count == virtualDisplays.Count)
            {
                // only virtual monitors are present  
                return;
            }

            // get main display  
            var mainDisplay = displays.FirstOrDefault(d => d.IsGDIPrimary);
            var mainScreen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == mainDisplay?.DisplayName);

            if (mainScreen == null)
            {
                mainScreen = Screen.PrimaryScreen;
            }

            var mainForm = new FrmMainUpdateWin10
            {
                StartPosition = FormStartPosition.Manual,
                Location = mainScreen.Bounds.Location,
                WindowState = FormWindowState.Maximized
            };
            mainForm.Show();

            if (displays.Count == 2)
            {
                Application.Run();
                return;
            }

            var virtualPaths = virtualDisplays.Select(v => v.DevicePath).ToHashSet();

            foreach (var screen in Screen.AllScreens)
            {
                var display = displays.FirstOrDefault(d => d.DisplayName == screen.DeviceName);
                if (display == null || display.IsGDIPrimary || virtualPaths.Contains(display.DevicePath))
                    continue;

                var blackOut = new FrmBlackOut
                {
                    StartPosition = FormStartPosition.Manual,
                    Location = screen.Bounds.Location,
                    WindowState = FormWindowState.Maximized
                };
                blackOut.Show();
            }

            Application.Run();
        }

        private static void KillExplorer()
        {
            try
            {
                Process[] explorers = Process.GetProcessesByName("explorer");
                
                if (explorers.Length > 0)
                {
                    explorerPid = explorers[0].Id;
                    
                    foreach (Process explorer in explorers)
                    {
                        explorer.Kill();
                        explorer.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to kill explorer: {ex.Message}");
            }
        }

        private static void RestoreExplorer()
        {
            try
            {
                Process.Start("explorer.exe");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to restart explorer: {ex.Message}");
            }
        }

        private static void DisableTaskManager(bool disable)
        {
            try
            {
                bool value = disable;
                SystemParametersInfo(SPI_SETSCREENSAVERRUNNING, disable ? 1u : 0u, ref value, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to {(disable ? "disable" : "enable")} Task Manager: {ex.Message}");
            }
        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            //bring the forms back to the front
            if (e.Reason == SessionSwitchReason.SessionUnlock || 
                e.Reason == SessionSwitchReason.ConsoleDisconnect ||
                e.Reason == SessionSwitchReason.RemoteDisconnect)
            {
                foreach (Form form in Application.OpenForms)
                {
                    form.TopMost = false;
                    form.TopMost = true;
                }
            }
        }
    }
}