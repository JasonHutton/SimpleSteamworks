using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ValveKeyValue;

namespace SimpleSteamworks
{
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
            if (string.IsNullOrEmpty(value))
            {
                throw new KeyNotFoundException();
            }

            return value;
        }

        // Clear the manifest cache.
        public void InvalidateCache()
        {
            _cache.Clear();
        }
    }
}
