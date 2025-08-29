using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;
using System.Diagnostics;

namespace MobitSystemInfoWidget
{
    public class SystemInfo
    {
        public string ComputerName { get; set; } = "N/A";
        public string Username { get; set; } = "N/A";
        public string Domain { get; set; } = "N/A";
        public string IpAddress { get; set; } = "N/A";
        public string MacAddress { get; set; } = "N/A";
        public string SerialNumber { get; set; } = "N/A";
        public string Manufacturer { get; set; } = "N/A";
        public string OsName { get; set; } = "N/A";
        public string OsVersion { get; set; } = "N/A";
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public static SystemInfo Gather()
        {
            var info = new SystemInfo();
            
            try
            {
                // Basic environment info
                info.ComputerName = Environment.MachineName ?? "N/A";
                info.Username = Environment.UserName ?? "N/A";
                
                // WMI-based system information
                GatherWmiInfo(info);
                
                // Network information
                GatherNetworkInfo(info);
                
                info.LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SystemInfo.Gather failed: {ex.Message}");
            }

            return info;
        }

        private static void GatherWmiInfo(SystemInfo info)
        {
            try
            {
                // Computer System info (Domain, Manufacturer)
                using (var searcher = new ManagementObjectSearcher("SELECT Domain, Manufacturer FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.Domain = obj["Domain"]?.ToString() ?? "Lokal Bruker";
                        info.Manufacturer = obj["Manufacturer"]?.ToString() ?? "N/A";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Win32_ComputerSystem query failed: {ex.Message}");
                info.Domain = "Lokal Bruker";
            }

            try
            {
                // BIOS info (Serial Number)
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.SerialNumber = obj["SerialNumber"]?.ToString() ?? "N/A";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Win32_BIOS query failed: {ex.Message}");
            }

            try
            {
                // Operating System info
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.OsName = obj["Caption"]?.ToString() ?? "N/A";
                        info.OsVersion = obj["Version"]?.ToString() ?? "N/A";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Win32_OperatingSystem query failed: {ex.Message}");
            }
        }

        private static void GatherNetworkInfo(SystemInfo info)
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .OrderBy(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ? 0 : 1);

                foreach (var ni in networkInterfaces)
                {
                    // Get MAC address
                    if (info.MacAddress == "N/A")
                    {
                        var mac = ni.GetPhysicalAddress().ToString();
                        if (!string.IsNullOrEmpty(mac) && mac != "000000000000")
                        {
                            info.MacAddress = string.Join("-", 
                                Enumerable.Range(0, mac.Length / 2)
                                         .Select(i => mac.Substring(i * 2, 2)));
                        }
                    }

                    // Get IP address - prefer DHCP addresses
                    var ipProperties = ni.GetIPProperties();
                    var unicastAddresses = ipProperties.UnicastAddresses
                        .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                                      !IPAddress.IsLoopback(addr.Address))
                        .OrderBy(addr => addr.IsDnsEligible ? 0 : 1);

                    foreach (var addr in unicastAddresses)
                    {
                        if (info.IpAddress == "N/A")
                        {
                            info.IpAddress = addr.Address.ToString();
                            break;
                        }
                    }

                    // If we have both MAC and IP, we can stop
                    if (info.MacAddress != "N/A" && info.IpAddress != "N/A")
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Network info gathering failed: {ex.Message}");
            }
        }
    }
}
