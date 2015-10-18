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
        private static AsyncAction bgAction;
        public static int progressNum;
        public static string progressDesc;
        private static bool fIsCASCInit = false;

        public static void InitCasc(AsyncAction bgAction = null, string basedir = null, string program = "wowt"){
            CASC.bgAction = bgAction;
            //bgAction.ProgressChanged += new EventHandler<AsyncActionProgressChangedEventArgs>(bgAction_ProgressChanged);
            if (basedir == null)
            {
                Console.WriteLine("Initializing CASC from web with program " + program);
                cascHandler = CASCHandler.OpenOnlineStorage(program, bgAction);
            }
            else
            {
                Console.WriteLine("Initializing CASC from local disk with basedir " + basedir);
                cascHandler = CASCHandler.OpenLocalStorage(basedir, bgAction);
            }
            
            cascHandler.Root.SetFlags(LocaleFlags.enUS, ContentFlags.None, false);

            fIsCASCInit = true;
        }

        private static void bgAction_ProgressChanged(object sender, AsyncActionProgressChangedEventArgs progress)
        {
            if (bgAction.IsCancellationRequested) { return; }
            progressNum = progress.Progress;
            if (progress.UserData != null) { progressDesc = progress.UserData.ToString(); }
        }

        public static List<string> GenerateListfile()
        {
            //extract signaturefile and extract list of some files from that

            List<string> files = new List<String>();


           /* if (!FileExists("signaturefile"))
            {
                new MissingFile("signaturefile");
                return files;
            }
            */
            string line;
            string[] linesplit;

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

            /*
            while ((line = file.ReadLine()) != null)
            {
                linesplit = line.Split(';');
                if (linesplit.Count() != 4) { continue; } //filter out junk 
                if (linesplit[3].EndsWith(".wmo", StringComparison.OrdinalIgnoreCase) || linesplit[3].EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(linesplit[3]);
                }
            }
            */
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

//#if DEBUG
//            if (Directory.Exists(@"Z:\WoW extracts") && File.Exists(Path.Combine(@"Z:\WoW extracts\20363_full", filename)))
//            {
//                return true;
//            }
//#endif

            return cascHandler.FileExists(filename);
        }

        public static Stream OpenFile(string filename)
        {

//#if DEBUG
//            var stream = File.OpenRead(Path.Combine(@"Z:\WoW extracts\20363_full", filename));

//            if (stream.CanRead) { return stream; }
//#endif
            return cascHandler.OpenFile(filename);
        }

        public static bool IsCASCInit { get { return fIsCASCInit; } }
    }
}
