using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace MobitSystemInfoWidget
{
    public static class StartupLogging
    {
        public static void WireUp()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("UnhandledException", e.ExceptionObject?.ToString() ?? "Unknown exception");
            Application.Current.DispatcherUnhandledException += (_, e) => 
            { 
                Log("DispatcherUnhandledException", e.Exception.ToString()); 
                e.Handled = true; 
            };
            
            Log("Startup",
                $"User={Environment.UserName}@{Environment.UserDomainName}\n" +
                $"CurrentDirectory={Environment.CurrentDirectory}\n" +
                $"BaseDirectory={AppContext.BaseDirectory}\n" +
                $"AssemblyLocation={Assembly.GetExecutingAssembly().Location}\n" +
                $"Time={DateTime.Now:O}");
        }

        public static void Log(string kind, string message)
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MobitSystemInfoWidget", "logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "startup.log"), $"[{DateTime.Now:O}] {kind}: {message}\n");
            }
            catch 
            { 
                // Swallow logging errors to prevent infinite loops
            }
        }
    }
}
