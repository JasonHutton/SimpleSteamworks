using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace SimpleSteamworks
{
    public class Steamworks
    {
        private string _steamInstallPath;

        private const string SteamRegistryKey = @"Software\Valve\Steam";
        private const string InstallPathValue = "InstallPath";

        public static string GetSteamInstallPath()
        {
            try
            {
                // First, attempt to get from 64-bit registry
                string path = GetRegistryValue(RegistryHive.LocalMachine, RegistryView.Registry64, SteamRegistryKey, InstallPathValue);
                if (!string.IsNullOrEmpty(path))
                    return Path.GetFullPath(path);

                // Fallback to 32-bit registry
                path = GetRegistryValue(RegistryHive.LocalMachine, RegistryView.Registry32, SteamRegistryKey, InstallPathValue);
                if (!string.IsNullOrEmpty(path))
                    return Path.GetFullPath(path);

                // Steam may not be installed
                return null;
            }
            catch (System.Security.SecurityException)
            {
                // Handle insufficient permissions
                return null;
            }
            catch
            {
                // Handle other exceptions
                return null;
            }
        }
        private static string GetRegistryValue(RegistryHive hive, RegistryView view, string subKey, string valueName)
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
            using (RegistryKey key = baseKey.OpenSubKey(subKey))
            {
                return key?.GetValue(valueName)?.ToString();
            }
        }
    }
}
