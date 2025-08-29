Given:

It runs when launched from your project/publish folder (where the working directory is the same as the exe’s folder).
It “flickers” with a tiny 0.1 MB process when launched via your deployment/startup method.
Our diagnostic explicitly set WorkingDirectory to the install folder, and it looked fine.
This points strongly to a working directory and/or write-location issue when launched outside the project folder.

Why this happens

If the app loads any files via relative paths (e.g., “Assets/bg.png”, “settings.json”), those resolve against the process’s current directory.
From your project/publish folder, that works.
From HKCU Run or some RMM launch contexts, CurrentDirectory is often C:\Windows\System32 (or something else), so relative file loads fail immediately and the app exits. That’s the tiny 0.1 MB blip you’re seeing.
Program Files is read-only for standard users; if the app tries to write next to the exe (logs/settings), it will fail unless you write to AppData/LocalAppData.
How to confirm quickly

Temporarily add a startup log on app launch that writes to %LOCALAPPDATA%\MobitSystemInfoWidget\startup.log with:
Environment.CurrentDirectory
AppContext.BaseDirectory
Assembly.GetExecutingAssembly().Location
Any exceptions on startup
Practical fixes

A) Make startup deterministic (recommended now)

Don’t rely on HKCU Run to launch the exe directly (it won’t set a Start In/working directory).
Use a Startup shortcut (.lnk) with Start In set to the install folder, or a tiny launcher .cmd that cd’s into the install folder before starting the exe.
param(
  [string]$InstallPath = "$env:ProgramFiles\MobitSystemInfoWidget",
  [string]$ExeName = "MobitSystemInfoWidget.exe"
)

$exe = Join-Path $InstallPath $ExeName

# Remove HKCU Run entry to avoid duplicate launches
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "MobitSystemInfoWidget" -ErrorAction SilentlyContinue

# Create Startup folder shortcut with proper Start In
$startup = [Environment]::GetFolderPath('Startup') # per-user Startup
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut((Join-Path $startup "Mobit System Info Widget.lnk"))
$link.TargetPath = $exe
$link.WorkingDirectory = $InstallPath
$link.Arguments = ""
$link.WindowStyle = 7
$link.Save()

Write-Host "Startup shortcut created at: $startup"

Optional: add a tiny launcher if you prefer
@echo off
cd /d "%~dp0"
start "" "MobitSystemInfoWidget.exe"


B) Harden the app (recommended going forward)

Resolve all file paths from AppContext.BaseDirectory (for read-only assets) or from a user-writable location for settings/logs.
Never depend on Environment.CurrentDirectory.
Don’t write to Program Files; write to %LOCALAPPDATA%\MobitSystemInfoWidget (or %APPDATA% if roaming is desired).
using System;
using System.IO;

public static class PathHelpers
{
    public static string BaseDir => AppContext.BaseDirectory;

    public static string Asset(string relative) =>
        Path.Combine(BaseDir, relative);

    public static string LocalDataDir {
        get {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MobitSystemInfoWidget");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string LocalData(string relative) =>
        Path.Combine(LocalDataDir, relative);
}

Usage examples:

Read-only asset: File.OpenRead(PathHelpers.Asset("Assets\bg.png"))
Settings/logs: PathHelpers.LocalData("settings.json") or PathHelpers.LocalData("logs\app.log")
C) Add minimal startup logging to catch this class of issues next time

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

public static class StartupLogging
{
    public static void WireUp()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("UnhandledException", e.ExceptionObject?.ToString());
        Application.Current.DispatcherUnhandledException += (_, e) => { Log("DispatcherUnhandledException", e.Exception.ToString()); e.Handled = true; };
        Log("Startup",
            $"User={Environment.UserName}@{Environment.UserDomainName}\n" +
            $"CurrentDirectory={Environment.CurrentDirectory}\n" +
            $"BaseDirectory={AppContext.BaseDirectory}\n" +
            $"AssemblyLocation={Assembly.GetExecutingAssembly().Location}\n" +
            $"Time={DateTime.Now:O}");
    }

    private static void Log(string kind, string message)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MobitSystemInfoWidget", "logs");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "startup.log"), $"[{DateTime.Now:O}] {kind}: {message}\n");
        }
        catch { /* swallow */ }
    }
}

Then call StartupLogging.WireUp() at the earliest point in App.OnStartup.

Why the diagnostic looked “fine”

In my scripts I started the exe with Start-Process -WorkingDirectory $InstallPath, which made relative path loads succeed. That’s why it ran for 10 seconds and showed ~71 MB.
The “flicker” you see elsewhere is likely the same exe starting with the wrong working directory, failing immediately, and potentially being relaunched by your RMM or a duplicate startup entry.
Next steps

Run Create-StartupShortcut.ps1 and remove the HKCU Run entry (script above already removes it).
Try launching via the new Startup shortcut or double-clicking the exe in Program Files (that should also work because Explorer sets Start In to the folder).
If you still see flicker, enable the startup logging above and share the log lines for CurrentDirectory and BaseDirectory.
Also, small note: the “op_Division” error you saw earlier was just my script trying to divide an array by 1MB when multiple processes matched; unrelated to your app. If you want that fixed script too, I can share it, but it’s not impacting the widget.

I’m confident this is a working-directory and/or write-location difference between your project folder and Program Files plus the way it’s launched at startup. The shortcut approach and path hardening will resolve it.