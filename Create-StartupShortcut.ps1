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
Write-Host "Removed registry-based startup entry (if it existed)"
Write-Host "The widget will now launch properly with correct working directory"
