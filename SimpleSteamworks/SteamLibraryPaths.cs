using System;
using System.Collections.Generic;
using System.IO;
using ValveKeyValue;

namespace SimpleSteamworks
{
    public class LibraryManifests
    {
        public string SteamInstallPath { get; }

        // Cache keyed by file path for caching the library manifest, despite there probably(?) only ever being one of these
        // and then by library keys (like "0", "1", etc.) when iterating its children.
        private readonly Cache<string, KVObject> _cache = new Cache<string, KVObject>();

        public LibraryManifests(string steamInstallPath)
        {
            SteamInstallPath = steamInstallPath;
        }

        public KVObject GetLibraryFoldersManifest()
        {
            string libraryFoldersPath = Path.Combine(SteamInstallPath, "steamapps", "libraryfolders.vdf");
            if (_cache.ContainsKey(libraryFoldersPath))
                return _cache[libraryFoldersPath];

            if (!File.Exists(libraryFoldersPath))
                return null;

            using (var stream = File.OpenRead(libraryFoldersPath))
            {
                var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
                KVObject manifest = kv.Deserialize(stream);
                _cache[libraryFoldersPath] = manifest;
                return manifest;
            }
        }

        // Retrieves the KVObject for a specific library within the libraryfolders manifest.
        public KVObject GetLibraryManifest(string libraryKey)
        {
            var manifest = GetLibraryFoldersManifest();
            if (manifest == null)
                return null;

            // Iterate over each child KVObject and check its Name property.
            foreach (var child in manifest)
            {
                if (child.Name == libraryKey)
                {
                    return child;
                }
            }
            return null;
        }

        /*public IEnumerable<string> GetLibraries()
        {
            var manifest = GetLibraryFoldersManifest();
            List<string> libraries = new List<string>();
            if (manifest != null)
            {
                // Each child's Name property represents the key (index) of the library.
                foreach (var child in manifest)
                {
                    libraries.Add(child.Name);
                }
            }
            return libraries;
        }*/

        // Retrieves a property value from a specific library manifest.
        public string GetLibraryManifestProperty(string libraryKey, string property)
        {
            KVObject libManifest = GetLibraryManifest(libraryKey);
            if (libManifest == null)
                return null;

            string value = libManifest[property]?.ToString();
            if (string.IsNullOrEmpty(value))
            {
                throw new KeyNotFoundException();
            }
            return value;
        }

        // Returns a list of all library paths extracted from the libraryfolders manifest.
        public List<string> GetLibraryPaths()
        {
            var manifest = GetLibraryFoldersManifest();
            List<string> paths = new List<string>();
            if (manifest != null)
            {
                // Iterate over each child and add the "path" property value if available.
                foreach (var child in manifest)
                {
                    if (child["path"] != null)
                    {
                        string path = Path.GetFullPath(child["path"].ToString());
                        paths.Add(path);
                    }
                }
            }
            return paths;
        }

        public string FindLibraryForApp(int appId)
        {
            List<string> libraryPaths = GetLibraryPaths();
            foreach (string library in libraryPaths)
            {
                string manifestPath = Path.Combine(library, "steamapps", $"appmanifest_{appId}.acf");
                if (File.Exists(manifestPath))
                    return library;
            }
            return null;
        }

        public void InvalidateCache()
        {
            _cache.InvalidateCache();
        }
    }
}
