using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Diagnostics;
using Microsoft.Win32;

namespace MobitSystemInfoWidget
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _refreshTimer;
        private SystemInfo? _currentInfo;

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up the refresh timer (30 minutes)
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMinutes(30);
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            // Initial load
            LoadSystemInfo();

            // Position once loaded to account for DPI/layout
            Loaded += (_, _) => PositionWindowSafe();

            // Keep position stable across display / power events
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            try
            {
                // Position window in top-right corner
                PositionWindow();
                
                // Set extended window styles
                var hwnd = new WindowInteropHelper(this).Handle;
                Win32Helper.SetWindowExtendedStyles(hwnd);
                Win32Helper.SetWindowToBottom(hwnd);
            }
            catch (Exception ex)
            {
                StartupLogging.Log("WindowInitError", $"Window initialization failed: {ex.Message}");
                Debug.WriteLine($"Window initialization failed: {ex.Message}");
            }
        }

        private void PositionWindow()
        {
            // Get screen dimensions
            var workArea = SystemParameters.WorkArea;
            var margin = 15;

            // Position 15px from top-right of the working area (respects taskbar)
            Left = Math.Max(workArea.Left, workArea.Right - Width - margin);
            Top = Math.Max(workArea.Top, workArea.Top + margin);
        }

        private void PositionWindowSafe()
        {
            try
            {
                PositionWindow();
                var hwnd = new WindowInteropHelper(this).Handle;
                Win32Helper.SetWindowToBottom(hwnd);
            }
            catch (Exception ex)
            {
                StartupLogging.Log("PositionError", $"PositionWindowSafe failed: {ex.Message}");
                Debug.WriteLine($"PositionWindowSafe failed: {ex.Message}");
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            PositionWindowSafe();
        }

        private void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
        {
            // Re-apply position when user returns (e.g., unlocks)
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                PositionWindowSafe();
            }
        }

        private void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
        {
            // Re-apply after resume from sleep/hibernate
            if (e.Mode == PowerModes.Resume)
            {
                PositionWindowSafe();
            }
        }

        private void LoadSystemInfo()
        {
            try
            {
                _currentInfo = SystemInfo.Gather();
                UpdateUI();
            }
            catch (Exception ex)
            {
                StartupLogging.Log("LoadSystemInfoError", $"LoadSystemInfo failed: {ex.Message}");
                Debug.WriteLine($"LoadSystemInfo failed: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            if (_currentInfo == null) return;

            try
            {
                // Handle logo fallback
                try
                {
                    // Try to load the embedded logo
                    LogoImage.Visibility = Visibility.Visible;
                    LogoFallback.Visibility = Visibility.Collapsed;
                }
                catch
                {
                    // Show fallback text if logo fails to load
                    LogoImage.Visibility = Visibility.Collapsed;
                    LogoFallback.Visibility = Visibility.Visible;
                }

                // Update system information
                ComputerNameText.Text = $"Datanavn: {_currentInfo.ComputerName}";
                UsernameText.Text = $"Bruker: {_currentInfo.Username}";
                DomainText.Text = $"Domene: {_currentInfo.Domain}";
                IpText.Text = $"IP-adresse: {_currentInfo.IpAddress}";
                MacText.Text = $"MAC-adresse: {_currentInfo.MacAddress}";
                SerialText.Text = $"SN: {_currentInfo.SerialNumber}";
                ManufacturerText.Text = $"Produsent: {_currentInfo.Manufacturer}";
                OsNameText.Text = $"Operativsystem: {_currentInfo.OsName}";
                OsVersionText.Text = $"Versjon: {_currentInfo.OsVersion}";
                TimestampText.Text = $"Sist oppdatert: {_currentInfo.LastUpdated:dd.MM.yyyy HH:mm}";

                // Ensure window stays at bottom after update
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        var hwnd = new WindowInteropHelper(this).Handle;
                        Win32Helper.SetWindowToBottom(hwnd);
                    }
                    catch (Exception ex)
                    {
                        StartupLogging.Log("PositionError", $"Failed to maintain bottom position: {ex.Message}");
                        Debug.WriteLine($"Failed to maintain bottom position: {ex.Message}");
                    }
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                StartupLogging.Log("UpdateUIError", $"UpdateUI failed: {ex.Message}");
                Debug.WriteLine($"UpdateUI failed: {ex.Message}");
            }
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            LoadSystemInfo();
        }

        protected override void OnActivated(EventArgs e)
        {
            // Prevent the window from becoming active
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                Win32Helper.SetWindowToBottom(hwnd);
            }
            catch (Exception ex)
            {
                StartupLogging.Log("ActivatedError", $"OnActivated positioning failed: {ex.Message}");
                Debug.WriteLine($"OnActivated positioning failed: {ex.Message}");
            }
            
            base.OnActivated(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;

            _refreshTimer?.Stop();
            _refreshTimer = null;
            base.OnClosed(e);
        }
    }
}
