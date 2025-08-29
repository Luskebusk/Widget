# Mobit System Info Widget

A lightweight C# WPF desktop widget that displays system information for IT support services.

## Features

- **Always-on display** in top-right corner of screen
- **Non-intrusive** - stays behind other windows, no taskbar/Alt+Tab presence
- **System information** including computer name, user, domain, IP/MAC addresses, hardware details
- **Auto-refresh** every 30 minutes
- **Single instance** enforcement with proper mutex handling
- **Startup shortcut** with proper working directory (replaces registry method)
- **Single-file deployment** ready for NinjaRMM
- **Comprehensive logging** for troubleshooting deployment issues

## Building

1. Ensure .NET 6 SDK or later is installed
2. Place your `logo.png` file in the `Resources` directory (optional - shows fallback text if missing)
3. Run `build.bat` to create a single-file executable

The output will be: `bin\Release\net6.0-windows\win-x64\publish\MobitSystemInfoWidget.exe`

## Deployment

The built executable is self-contained and can be deployed via NinjaRMM. **Important**: After deployment, run the included PowerShell script to set up proper startup:

```powershell
# Run this after copying the exe to Program Files
.\Create-StartupShortcut.ps1 -InstallPath "C:\Program Files\MobitSystemInfoWidget"
```

This will:
- Remove any old registry-based startup entries
- Create a proper startup shortcut with correct working directory
- Prevent the "flicker" issue when launched at startup

## Troubleshooting

If the widget appears to start and immediately exit (flickers), check the startup log:
```
%LOCALAPPDATA%\MobitSystemInfoWidget\logs\startup.log
```

This log will show:
- Working directory issues
- File loading problems  
- Startup exceptions
- Environment details

## Technical Details

- **Framework**: .NET 6 WPF
- **Single file size**: ~40-50MB (self-contained)
- **Memory usage**: <50MB RAM
- **CPU usage**: <1% average
- **Refresh interval**: 30 minutes
- **Mutex name**: `Global\MobitSystemInfoWidget_B4C7F2E1-8A9D-4F3B-9C2E-1A2B3C4D5E6F`
- **Logs location**: `%LOCALAPPDATA%\MobitSystemInfoWidget\logs\`

## Files Structure

```
MobitSystemInfoWidget.csproj  - Project file with dependencies
App.xaml / App.xaml.cs        - Application entry point and mutex handling
MainWindow.xaml               - UI layout (exactly as specified)
MainWindow.xaml.cs            - Main window logic and system info display
SystemInfo.cs                 - System information gathering via WMI
Win32Helper.cs                - Windows API calls for window positioning
PathHelpers.cs                - Proper path resolution (fixes working directory issues)
StartupLogging.cs             - Comprehensive startup and error logging
Resources/logo.png             - Company logo (embedded resource)
build.bat                     - Build script for single-file deployment
Create-StartupShortcut.ps1    - PowerShell script for proper startup setup
launcher.cmd                  - Optional batch launcher (alternative method)
```

## Key Improvements

- **Path hardening**: Uses `AppContext.BaseDirectory` instead of current directory
- **Proper logging**: All startup issues and exceptions are logged to LocalAppData
- **Startup shortcut**: Replaces unreliable registry method with proper .lnk file
- **Working directory independence**: App works regardless of where it's launched from
- **Exception handling**: Graceful handling of all system info gathering and UI operations
