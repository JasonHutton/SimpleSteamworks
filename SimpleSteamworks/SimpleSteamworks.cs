using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using ValveKeyValue;

namespace SimpleSteamworks
{
    public class SteamLibraryPaths
    {
        public List<string> Paths { get; }
        public string SteamInstallPath { get; }

        private KVObject _libraryObjects;

        public SteamLibraryPaths(string steamInstallPath)
        {
            SteamInstallPath = steamInstallPath;
            Paths = new List<string>();
        }

        public void Add(string path) => Paths.Add(path);
        public void Add(IEnumerable<string> paths) => Paths.AddRange(paths);

        public void LoadSteamLibraries()
        {
            string libraryFoldersPath = Path.Combine(SteamInstallPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersPath))
                return;

            var stream = File.OpenRead(libraryFoldersPath);

            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            _libraryObjects = kv.Deserialize(stream);

            foreach(var property in _libraryObjects)
            {
                if (property["path"] != null)
                {
                    string libraryPath = property["path"].ToString();
                    Add(libraryPath);
                }
            }
        }
    }

    public class AppManifestCache
    {
        // Cache mapping app id -> parsed manifest KV object.
        private readonly Dictionary<int, KVObject> _cache = new Dictionary<int, KVObject>();

        // Reference to the loaded Steam library paths.
        private readonly SteamLibraryPaths _libraryPaths;

        // A KV serializer configured for Valve's text format.
        private readonly KVSerializer _kvSerializer;

        public AppManifestCache(SteamLibraryPaths libraryPaths)
        {
            _libraryPaths = libraryPaths ?? throw new ArgumentNullException(nameof(libraryPaths));
            _kvSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        }

        // Returns the manifest for the given appId, loading and caching it if needed.
        public KVObject GetAppManifest(int appId)
        {
            if (_cache.ContainsKey(appId))
                return _cache[appId];

            // Iterate over each Steam library path.
            foreach (string library in _libraryPaths.Paths)
            {
                string manifestPath = Path.Combine(library, "steamapps", $"appmanifest_{appId}.acf");
                if (File.Exists(manifestPath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(manifestPath))
                        {
                            KVObject manifest = _kvSerializer.Deserialize(stream);
                            _cache[appId] = manifest;
                            return manifest;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading manifest for app {appId} in '{library}': {ex.Message}");
                    }
                }
            }
            // If not found in any library, return null.
            return null;
        }

        public string GetManifestProperty(int appId, string property)
        {
            KVObject manifest = GetAppManifest(appId);
            if (manifest == null)
                return null;

            string value = manifest[property]?.ToString();
            if(string.IsNullOrEmpty(value))
            {
                throw new KeyNotFoundException();
            }

            return value;
        }
    }

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
                    return path;

                // Fallback to 32-bit registry
                path = GetRegistryValue(RegistryHive.LocalMachine, RegistryView.Registry32, SteamRegistryKey, InstallPathValue);
                if (!string.IsNullOrEmpty(path))
                    return path;

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
