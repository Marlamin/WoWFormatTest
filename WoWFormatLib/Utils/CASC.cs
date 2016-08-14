using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CASCExplorer;
using System.IO;
using WoWFormatLib.DBC;

namespace WoWFormatLib.Utils
{
    public class CASC
    {
        public static CASCHandler cascHandler;

        private static bool fIsCASCInit = false;
        public static Dictionary<int, string> rootList;
        public static void InitCasc(BackgroundWorkerEx worker = null, string basedir = null, string program = "wowt"){

            CASCConfig.LoadFlags &= ~(LoadFlags.Download | LoadFlags.Install);
            CASCConfig.ValidateData = false;
            CASCConfig.ThrowOnFileNotFound = false;
            
            if (basedir == null)
            {
                CASCConfig config = CASCConfig.LoadOnlineStorageConfig(program, "us", true);

                Console.WriteLine("Initializing CASC from web with program " + program + " and build " + config.BuildName);
                cascHandler = CASCHandler.OpenStorage(config, worker);
            }
            else
            {
                Console.WriteLine("Initializing CASC from local disk with basedir " + basedir);
                cascHandler = CASCHandler.OpenLocalStorage(basedir, worker);
            }
            
            cascHandler.Root.SetFlags(LocaleFlags.enUS, ContentFlags.None, false);

            fIsCASCInit = true;
        }

        public static List<string> GenerateListfile()
        {
            List<string> files = new List<String>();

            string line;

            // System.IO.StreamReader file = new System.IO.StreamReader(CASC.OpenFile("signaturefile"));

            if (CASC.FileExists("DBFilesClient\\FileData.dbc"))
            {
                DBCReader<FileDataRecord> filedatareader = new DBCReader<FileDataRecord>("DBFilesClient\\FileData.dbc");

                for (int i = 0; i < filedatareader.recordCount; i++)
                {
                    string filename = filedatareader[i].FilePath + filedatareader[i].FileName;
                    if (filename.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                    {
                        files.Add(filename);
                    }
                }
            }

            if (File.Exists("wow-live-listfile-seed.txt"))
            {
                var file = new StreamReader("wow-live-listfile-seed.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase) || line.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                    {
                        files.Add(line);
                    }
                }
            }
            
            List<string> unwantedExtensions = new List<String>();
            for (int i = 0; i < 1024; i++)
            {
                unwantedExtensions.Add("_" + i.ToString().PadLeft(3, '0') + ".wmo");
                unwantedExtensions.Add("_" + i.ToString().PadLeft(3, '0') + ".WMO");
            }

            string[] unwanted = unwantedExtensions.ToArray();
            List<string> retlist = new List<string>();

            for (int i = 0; i < files.Count(); i++)
            {
                if (!files[i].StartsWith("alternate") && !files[i].StartsWith("Camera"))
                {
                    if (!unwanted.Contains(files[i].Substring(files[i].Length - 8, 8)))
                    {
                        if (!files[i].Contains("LOD"))
                        {
                            retlist.Add(files[i]);
                        }
                    }
                }
            }

            return retlist.Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        public static bool FileExists(string filename)
        {
            if (string.IsNullOrEmpty(filename)) { return false; }

            return cascHandler.FileExists(filename);
        }

        //Why do I even do this shit
        public static bool FileExists(int fileDataID)
        {
            return cascHandler.FileExists(fileDataID);
        }

        public static Stream OpenFile(string filename)
        {
            return cascHandler.OpenFile(filename);
        }

        public static Stream OpenFile(int fileDataID)
        {
            return cascHandler.OpenFile(fileDataID);
        }

        public static int getFileDataIdByName(string name)
        {
            return (int)(cascHandler.Root as WowRootHandler)?.GetFileDataIdByName(name);
        }

        public static bool IsCASCInit { get { return fIsCASCInit; } }
    }
}
