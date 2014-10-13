using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CASCExplorer;

namespace CASCtest
{
    class Program
    {
        //wow_beta for WoD Beta, wowt for 6.0 PTR, wow for live/preload
        private static string programcode = "wow_beta";

        static void Main(string[] args)
        {
            CASCHandler CASC = CASCHandler.OpenOnlineStorage("wow_beta");
            CASC.Root.SetFlags(LocaleFlags.enUS, ContentFlags.None);
            CASC.SaveFileTo("DBFilesClient\\Spell.dbc", ".", LocaleFlags.enUS, ContentFlags.None);
        }
        /*
        //Versions values
        public static string buildConfigHash;
        public static string cdnConfigHash;
        public static string buildID;
        public static string versionsName;
        
        // CDN configuration values
        public static string archiveGroup;
        public static string[] archives;

        // Build configuration values
        public static string rootHash;
        public static string downloadHash;
        public static string installHash;
        public static string[] encodingHashes;
        public static string[] encodingSizes;

        static void Main(string[] args)
        {
            LoadVersions();
            Console.WriteLine("Loading patch " + versionsName);
            LoadCDNconfig(cdnConfigHash);
            Console.WriteLine("Retrieved config file, " + archives.Count() +" archives listed!");
            LoadBuildConfig(buildConfigHash);
            Console.WriteLine("Retrieved build file!");
            Console.WriteLine("Downloading indexes..");
            if (!Directory.Exists("indexes")) { Directory.CreateDirectory("indexes"); }
            for (int i = 0; i < archives.Count(); i++)
            {
                if (!File.Exists("indexes/" + archives[i] + ".index"))
                {
                    Console.Write("Downloading " + archives[i] + ".index..");
                    CascUtils.DownloadFileFromCDN("data/" + archives[i][0] + archives[i][1] + "/" + archives[i][2] + archives[i][3] + "/" + archives[i] + ".index", "indexes/" + archives[i] + ".index");
                    Console.Write(" done!\n");
                }
            }
            Console.WriteLine("Indexes downloaded!");
            if (!Directory.Exists("data")) { Directory.CreateDirectory("data"); }
            
            for (int i = 0; i < encodingHashes.Count(); i++)
            {
                Console.Write("Downloading encoding #" + i + "..");
                if(!File.Exists("data/" + encodingHashes[i])){

                    CascUtils.DownloadFileFromCDN("data/" + encodingHashes[i][0] + encodingHashes[i][1] + "/" + encodingHashes[i][2] + encodingHashes[i][3] + "/" + encodingHashes[i], "data/" + encodingHashes[i]);
                    Console.Write(" done!\n");
                }
                else
                {
                    Console.Write(" already exists!\n");
                }
            }

            Console.WriteLine("Loading encoding..");
            CascUtils.ParseEncoding(encodingHashes);
            Console.WriteLine("Loaded encoding!");

            Console.ReadLine();
         *
        } 

        private static void LoadCDNconfig(string configHash)
        {
            //TODO Patch archives
            byte[] cdnconfig = CascUtils.DownloadFileFromCDN("config/" + configHash[0] + configHash[1] + "/" + configHash[2] + configHash[3] + "/" + configHash);
            StreamReader file = new StreamReader(new MemoryStream(cdnconfig));
            if (file.ReadLine() != "# CDN Configuration") {
                throw new Exception("CDN configuration has invalid header!");
            }
            else
            {
                archiveGroup = file.ReadLine().Replace("archive-group = ", "");
                archives = file.ReadLine().Replace("archives = ","").Split(' ');
            }
        }

        private static void LoadBuildConfig(string confighash)
        {
            byte[] cdnconfig = CascUtils.DownloadFileFromCDN("config/" + confighash[0] + confighash[1] + "/" + confighash[2] + confighash[3] + "/" + confighash);
            StreamReader file = new StreamReader(new MemoryStream(cdnconfig));

            if (file.ReadLine() != "# Build Configuration")
            {
                throw new Exception("Build configuration has invalid header!");
            }
            else
            {
                file.ReadLine(); //empty line
                rootHash = file.ReadLine().Replace("root = ", "");
                downloadHash = file.ReadLine().Replace("download = ", "");
                installHash = file.ReadLine().Replace("install = ","");
                encodingHashes = file.ReadLine().Replace("encoding = ", "").Split(' ');
                encodingSizes = file.ReadLine().Replace("encoding-size = ", "").Split(' ');
            }
        }

        private static void LoadVersions()
        {
            string line;
            string[] values;

            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] = "Marlamin's CASC thing";

                byte[] arr = client.DownloadData("http://us.patch.battle.net/" + programcode + "/versions");
                StreamReader file = new StreamReader(new MemoryStream(arr));

                if (file.ReadLine() != "Region!STRING:0|BuildConfig!HEX:16|CDNConfig!HEX:16|BuildId!DEC:4|VersionsName!String:0")
                {
                    throw new Exception("Unknown versions file, header is bad");
                }
                else
                {
                    //Just read the first entry for now, it's probably US
                    line = file.ReadLine();
                    values = line.Split('|');
                    buildConfigHash = values[1];
                    cdnConfigHash = values[2];
                    buildID = values[3];
                    versionsName = values[4];
                }
            }
        }*/
    }
}
