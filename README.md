# Mobit System Info Widget

A lightweight C# WPF desktop widget that displays system information for IT support services.

## Features

- **Always-on display** in top-right corner of screen
- **Non-intrusive** - stays behind other windows, no taskbar/Alt+Tab presence
- **System information** including computer name, user, domain, IP/MAC addresses, hardware details
- **Auto-refresh** every 30 minutes
- **Single instance** enforcement with proper mutex handling
- **Auto-startup** registration in Windows registry
- **Single-file deployment** ready for NinjaRMM

## Building

1. Ensure .NET 6 SDK or later is installed
2. Place your `logo.png` file in the `Resources` directory (optional - shows fallback text if missing)
3. Run `build.bat` to create a single-file executable

The output will be: `bin\Release\net6.0-windows\win-x64\publish\MobitSystemInfoWidget.exe`

## Deployment

The built executable is self-contained and can be deployed via NinjaRMM. On first run, it will:
- Register itself for auto-startup
- Position itself in the top-right corner
- Begin displaying system information

## Technical Details

- **Framework**: .NET 6 WPF
- **Single file size**: ~40-50MB (self-contained)
- **Memory usage**: <50MB RAM
- **CPU usage**: <1% average
- **Refresh interval**: 30 minutes
- **Mutex name**: `Global\MobitSystemInfoWidget_B4C7F2E1-8A9D-4F3B-9C2E-1A2B3C4D5E6F`

## Files Structure

```
MobitSystemInfoWidget.csproj  - Project file with dependencies
App.xaml / App.xaml.cs        - Application entry point and mutex handling
MainWindow.xaml               - UI layout (exactly as specified)
MainWindow.xaml.cs            - Main window logic and system info display
SystemInfo.cs                 - System information gathering via WMI
Win32Helper.cs                - Windows API calls for window positioning
Resources/logo.png             - Company logo (embedded resource)
build.bat                     - Build script for single-file deployment
```
