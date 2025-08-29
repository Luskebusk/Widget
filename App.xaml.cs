using System;
using System.Threading;
using System.Windows;
using System.Diagnostics;

namespace MobitSystemInfoWidget
{
    public partial class App : Application
    {
        private const string MutexName = @"Global\MobitSystemInfoWidget_B4C7F2E1-8A9D-4F3B-9C2E-1A2B3C4D5E6F";
        private Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Wire up startup logging first thing
            StartupLogging.WireUp();

            // Ensure single instance
            if (!CreateMutex())
            {
                StartupLogging.Log("SingleInstance", "Another instance is already running. Exiting.");
                Shutdown();
                return;
            }

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
            // Auto-start is now handled by Create-StartupShortcut.ps1
            // This method is kept for backward compatibility but does nothing
            try
            {
                StartupLogging.Log("AutoStart", "Auto-start registration skipped - using startup shortcut method");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auto-start registration info failed: {ex.Message}");
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
