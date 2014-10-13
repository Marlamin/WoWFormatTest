using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CASCtest
{
    class Program
    {
        private static string programcode = "wow_beta";
        public static string buildconfighash;
        public static string cdnconfighash;
        public static string buildid;
        public static string versionsname;
        
        // CDN configuration values
        public static string archivegroup;
        public static string[] archives;

        // Build configuration values
        public static string roothash;
        public static string downloadhash;
        public static string installhash;
        public static string[] encodinghashes;
        public static string[] encodingsizes;

        static void Main(string[] args)
        {
            LoadVersions();
            Console.WriteLine("Loading patch " + versionsname);
            LoadCDNconfig(cdnconfighash);
            Console.WriteLine("Retrieved config file, " + archives.Count() +" archives listed!");
            LoadBuildConfig(buildconfighash);
            Console.WriteLine("Retrieved build file!");
            Console.WriteLine("Downloading indexes..");
            if (!Directory.Exists("indexes")) { Directory.CreateDirectory("indexes"); }
            for (int i = 0; i < archives.Count(); i++)
            {
                if (!File.Exists("indexes/" + archives[i] + ".index"))
                {
                    Console.Write("Downloading " + archives[i] + ".index..");
                    DownloadFileFromCDN("data/" + archives[i][0] + archives[i][1] + "/" + archives[i][2] + archives[i][3] + "/" + archives[i] + ".index", "indexes/" + archives[i] + ".index");
                    Console.Write(" done!\n");
                }
            }
            Console.WriteLine("Indexes downloaded!");
            if (!Directory.Exists("data")) { Directory.CreateDirectory("data"); }
            
            for (int i = 0; i < encodinghashes.Count(); i++)
            {
                Console.Write("Downloading encoding #" + i + "..");
                if(!File.Exists("data/" + encodinghashes[i])){
                    
                    DownloadFileFromCDN("data/" + encodinghashes[i][0] + encodinghashes[i][1] + "/" + encodinghashes[i][2] + encodinghashes[i][3] + "/" + encodinghashes[i], "data/" + encodinghashes[i]);
                    Console.Write(" done!\n");
                }
                else
                {
                    Console.Write(" already exists!\n");
                }
            }
            Console.ReadLine();
        }

        private static void LoadCDNconfig(string confighash)
        {
            //TODO Patch archives
            byte[] cdnconfig = DownloadFileFromCDN("config/" + confighash[0] + confighash[1] + "/" + confighash[2] + confighash[3] + "/" + confighash);
            StreamReader file = new StreamReader(new MemoryStream(cdnconfig));
            if (file.ReadLine() != "# CDN Configuration") {
                throw new Exception("CDN configuration has invalid header!");
            }
            else
            {
                archivegroup = file.ReadLine().Replace("archive-group = ", "");
                var archivelist = file.ReadLine().Replace("archives = ","").Split(' ');
                archives = new string[archivelist.Count()];
                archives = archivelist;
            }
        }

        private static void LoadBuildConfig(string confighash)
        {
            //TODO Patch archives
            byte[] cdnconfig = DownloadFileFromCDN("config/" + confighash[0] + confighash[1] + "/" + confighash[2] + confighash[3] + "/" + confighash);
            StreamReader file = new StreamReader(new MemoryStream(cdnconfig));

            if (file.ReadLine() != "# Build Configuration")
            {
                throw new Exception("Build configuration has invalid header!");
            }
            else
            {
                file.ReadLine(); //empty line
                roothash = file.ReadLine().Replace("root = ", "");
                downloadhash = file.ReadLine().Replace("download = ", "");
                installhash = file.ReadLine().Replace("install = ","");
                encodinghashes = file.ReadLine().Replace("encoding = ", "").Split(' ');
                encodingsizes = file.ReadLine().Replace("encoding-size = ", "").Split(' ');
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
                    buildconfighash = values[1];
                    cdnconfighash = values[2];
                    buildid = values[3];
                    versionsname = values[4];
                }
            }
        }

        private static byte[] DownloadFileFromCDN(string path, string outputpath = "")
        {
            byte[] arr;
            //TODO path starts with data, check if file is present in local archives first, THEN web client
            using (WebClient client = new WebClient())
            {
                
                client.Headers["User-Agent"] = "Marlamin's CASC thing";
                if (outputpath.Count() > 0)
                {
                    try
                    {
                        client.DownloadFile("http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path, outputpath);
                    }
                    catch (System.Net.WebException e)
                    {
                        Console.WriteLine("[ERROR] 404 not found (" + "http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path + ")");
                    }
                    arr = new Byte[1];
                }
                else
                {
                    try
                    {
                        arr = client.DownloadData("http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path);
                    }
                    catch (System.Net.WebException e)
                    {
                        Console.WriteLine("[ERROR] 404 not found (" + "http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path + ")");
                        arr = new Byte[1];
                    }
                }
            }
            return arr;
        }
    }
}
