using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

            foreach (var property in _libraryObjects)
            {
                if (property["path"] != null)
                {
                    string libraryPath = property["path"].ToString();
                    Add(libraryPath);
                }
            }
        }
    }
}
