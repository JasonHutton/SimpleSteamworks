using System;
using System.Collections.Generic;
using System.IO;
using ValveKeyValue;

namespace SimpleSteamworks
{
    public class AppManifestCache
    {
        // Cache mapping app id -> parsed manifest KV object.
        private readonly Cache<int, KVObject> _cache = new Cache<int, KVObject>();

        // Reference to the loaded Steam library paths.
        private readonly LibraryManifests _libraryManifests;

        // A KV serializer configured for Valve's text format.
        private readonly KVSerializer _kvSerializer;

        public AppManifestCache(LibraryManifests libraryManifests)
        {
            _libraryManifests = libraryManifests ?? throw new ArgumentNullException(nameof(libraryManifests));
            _kvSerializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
        }

        // Returns the manifest for the given appId, loading and caching it if needed.
        public KVObject GetAppManifest(int appId)
        {
            if (_cache.ContainsKey(appId))
                return _cache[appId];

            List<string> libraryPaths = _libraryManifests.GetLibraryPaths();
            foreach (string library in libraryPaths)
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
            if (string.IsNullOrEmpty(value))
            {
                throw new KeyNotFoundException();
            }

            return value;
        }

        public string GetAppAbsoluteInstallPath(int appId)
        {
            var appLib = _libraryManifests.FindLibraryForApp(appId);
            var appInstallDir = GetManifestProperty(appId, "installdir");
            string appAbsoluteInstallPath = Path.Combine(appLib, "steamapps", "common", appInstallDir);

            return appAbsoluteInstallPath;
        }

        public void InvalidateCache()
        {
            _cache.InvalidateCache();
        }
    }
}
