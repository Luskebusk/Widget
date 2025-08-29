# C# WPF Desktop Widget - Development Instructions

## Project Requirements

Create a C# WPF desktop application that replaces the PowerShell-based widget with identical functionality and appearance.

## Core Features

### Visual Requirements
- **Exact same appearance** as the PowerShell widget using the provided XAML
- **Positioning**: Top-right corner of screen, 15px from edges
- **Always behind other windows** - never comes to front
- **Transparent background** with rounded corners and drop shadow
- **Non-interactive** - no moving, clicking, or focus stealing
- **Hidden from taskbar and Alt+Tab**

### Functionality Requirements

#### Single Instance Management
- **Proper mutex implementation** to prevent multiple instances
- Should handle abandoned mutex exceptions (crashed previous instances)
- Use unique GUID-based mutex name: `Global\MobitSystemInfoWidget_B4C7F2E1-8A9D-4F3B-9C2E-1A2B3C4D5E6F`

#### System Information Display
Collect and display these fields (updating every 30 minutes):
- **Computer name** (`Environment.MachineName`)
- **Current user** (`Environment.UserName`)
- **Domain** (WMI Win32_ComputerSystem.Domain, fallback to "Lokal Bruker")
- **IP address** (DHCP preferred, fallback to any non-127.0.0.1)
- **MAC address** (first active network adapter)
- **Serial number** (WMI Win32_BIOS.SerialNumber)
- **Manufacturer** (WMI Win32_ComputerSystem.Manufacturer)
- **OS name and version** (WMI Win32_OperatingSystem)
- **Last updated timestamp** (dd.MM.yyyy HH:mm format)

#### Logo Handling
- **Embedded resource**: Include logo.png as embedded resource
- **Fallback text**: Show "mobit IT-Tjenester" if logo fails to load
- **Error handling**: Graceful fallback without exceptions

#### Auto-Start Configuration
- **Registry startup entry**: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- **Key name**: "MobitSystemInfoWidget"
- **Value**: Full path to executable
- **Self-registration**: App should add its own startup entry on first run

#### Window Behavior
- **Always bottom layer**: Use Win32 SetWindowPos with HWND_BOTTOM
- **No activation**: WS_EX_NOACTIVATE and WS_EX_TOOLWINDOW extended styles
- **Timer refresh**: Update system info every 30 minutes
- **Keep position**: Maintain bottom layer position during refreshes

## Technical Specifications

### Framework
- **.NET 6 or higher** for single-file publishing support
- **WPF Application** project template
- **Windows target** (x64 recommended)

### Publishing Requirements
- **Single file executable**: Use PublishSingleFile=true
- **Self-contained**: Include .NET runtime (or require .NET 6+ installed)
- **Target size**: Keep under 50MB if possible
- **Output**: Single .exe file for NinjaRMM deployment

### Performance Requirements
- **Low CPU usage**: < 1% average CPU usage
- **Low memory footprint**: < 50MB RAM usage
- **Fast startup**: < 2 seconds to show widget
- **Reliable refresh**: Never crash during WMI calls

### Error Handling
- **Graceful degradation**: Show "N/A" for unavailable system info
- **WMI failures**: Handle timeout and access denied gracefully  
- **Network failures**: Don't crash if network adapters unavailable
- **Logging**: Optional console output for debugging (hidden in production)

## Deployment Integration

### NinjaRMM Compatibility
- **Silent execution**: No user prompts or dialogs
- **Return codes**: 0 for success, non-zero for failure
- **File placement**: Can be placed anywhere, self-contained
- **Admin rights**: Should work with both admin and user rights
- **Windows versions**: Support Windows 10 and 11

### Installation Behavior
- **First run**: Auto-register for startup
- **Cleanup**: Remove old instances before starting
- **Update friendly**: Can be replaced while running (mutex will prevent conflicts)

## Code Structure Suggestions

```
MainWindow.xaml     - Use provided XAML exactly
MainWindow.xaml.cs  - Window code-behind and system info logic
App.xaml           - Application definition
App.xaml.cs        - Single instance mutex and startup logic  
SystemInfo.cs      - System information gathering class
Win32Helper.cs     - P/Invoke declarations for window positioning
Resources/         - Embedded logo.png
```

## Success Criteria

1. **Identical appearance** to current PowerShell widget
2. **Zero duplicate instances** - mutex works 100% reliably  
3. **Successful NinjaRMM deployment** as single .exe file
4. **Auto-startup registration** works on target machines
5. **30-minute refresh cycle** maintains current timestamp
6. **Resource efficient** - no memory leaks or CPU spikes
7. **Crash resistant** - handles all WMI/network errors gracefully

## Contact Information
- **Email**: forde@mobit.no  
- **Phone**: +47 90 68 05 05

Use these exact values in the XAML - do not make them configurable.