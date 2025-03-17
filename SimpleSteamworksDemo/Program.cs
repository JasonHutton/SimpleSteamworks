using System.IO;
using SimpleSteamworks;

namespace SimpleSteamworksDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = Steamworks.GetSteamInstallPath();
            if(string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Steam installation not detected!");
                return;
            }
            Console.WriteLine("Steam is installed here: {0}", path);

            LibraryManifests libs = new LibraryManifests(path);
            if(libs.GetLibraryPaths().Count > 1)
            {
                Console.WriteLine("Steam Libraries are here:");
            }
            else
            {
                Console.WriteLine("Steam Library is here:");
            }
            //libs.GetLibraryPaths().ForEach(Console.WriteLine(Path.GetFullPath()));
            libs.GetLibraryPaths().ForEach(path => Console.WriteLine(Path.GetFullPath(path)));

            AppManifestCache apps = new AppManifestCache(libs);
            Console.WriteLine("App Manifest:");
            // You can find Steam App IDs easily here if you don't know them: https://steamdb.info/
            const int appId = 1549970; // This is Aliens: Fireteam Elite
            Console.WriteLine("{0}: {1}", "appid", apps.GetManifestProperty(appId, "appid"));
            Console.WriteLine("{0}: {1}", "name", apps.GetManifestProperty(appId, "name"));
            Console.WriteLine("{0}: {1}", "installdir", apps.GetManifestProperty(appId, "installdir"));

            string absInstallPath = apps.GetAppAbsoluteInstallPath(appId);
            Console.WriteLine("{0}: {1}", "Absolute App installdir", Path.GetFullPath(absInstallPath));

        }
    }
}
