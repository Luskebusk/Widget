using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace MobitSystemInfoWidget
{
    public partial class App : Application
    {
        private const string MutexName = @"Global\MobitSystemInfoWidget_B4C7F2E1-8A9D-4F3B-9C2E-1A2B3C4D5E6F";
        private Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Ensure single instance
            if (!CreateMutex())
            {
                MessageBox.Show("Another instance of Mobit System Info Widget is already running.", 
                    "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            // Register for auto-start
            RegisterAutoStart();

            base.OnStartup(e);
        }

        private bool CreateMutex()
        {
            try
            {
                _mutex = new Mutex(true, MutexName, out bool createdNew);
                
                if (!createdNew)
                {
                    // Another instance is running
                    _mutex?.Dispose();
                    _mutex = null;
                    return false;
                }

                return true;
            }
            catch (AbandonedMutexException)
            {
                // Previous instance crashed, we can take over
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Mutex creation failed: {ex.Message}");
                return false;
            }
        }

        private void RegisterAutoStart()
        {
            try
            {
                string executablePath = Environment.ProcessPath ?? "";
                if (string.IsNullOrEmpty(executablePath))
                    return;

                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);
                
                if (key != null)
                {
                    // Check if already registered
                    var currentValue = key.GetValue("MobitSystemInfoWidget") as string;
                    if (currentValue != executablePath)
                    {
                        key.SetValue("MobitSystemInfoWidget", executablePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-start registration failed: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
