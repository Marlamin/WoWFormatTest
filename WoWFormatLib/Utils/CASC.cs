using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CASCExplorer;
using System.IO;

namespace WoWFormatLib.Utils
{
    public class CASC
    {
        public static CASCHandler cascHandler;
        private static AsyncAction bgAction;
        public static int progressNum;
        public static string progressDesc;

        public static void InitCasc(AsyncAction bgAction = null){
            CASC.bgAction = bgAction;
            //bgAction.ProgressChanged += new EventHandler<AsyncActionProgressChangedEventArgs>(bgAction_ProgressChanged);
            cascHandler = CASCHandler.OpenOnlineStorage("wow_beta", bgAction);
            cascHandler.Root.SetFlags(LocaleFlags.enUS, ContentFlags.None, false);
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
            if (!FileExists("signaturefile"))
            {
                new MissingFile("signaturefile");
            }

            string line;
            string[] linesplit;
            List<string> files = new List<String>();

            System.IO.StreamReader file = new System.IO.StreamReader("data/signaturefile");

            List<string> unwantedExtensions = new List<String>();
            for (int i = 0; i < 1024; i++)
            {
                unwantedExtensions.Add("_" + i.ToString().PadLeft(3, '0') + ".wmo");
                unwantedExtensions.Add("_" + i.ToString().PadLeft(3, '0') + ".WMO");
            }

            string[] unwanted = unwantedExtensions.ToArray();

            //File.WriteAllText("listfile.txt", "");
            while ((line = file.ReadLine()) != null)
            {
                linesplit = line.Split(';');
                if (linesplit.Count() != 4) { continue; } //filter out junk 
                
                if (linesplit[3].EndsWith(".wmo", StringComparison.OrdinalIgnoreCase) || linesplit[3].EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                {
                    if (!linesplit[3].StartsWith("alternate"))
                    {
                        if (!unwanted.Contains(linesplit[3].Substring(linesplit[3].Length - 8, 8)))
                        {
                            if (!linesplit[3].Contains("LOD"))
                            { 
                                files.Add(linesplit[3]); 
                            }
                        }
                        
                    }
                }

            }
           // File.AppendAllLines("listfile.txt", files.ToArray());
            return files;
        }

        public static void DownloadFile(string filename)
        {
            try
            {
                cascHandler.SaveFileTo(filename, "data/");
                //Console.WriteLine("Downloaded " + filename + "!");
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine("Couldn't download " + filename + ", file was not found!");
            }
        }

        public static bool FileExists(string filename)
        {
            if (!File.Exists(Path.Combine("data", filename)))
            {
                //Console.WriteLine("File does not exist! Downloading.. (" + filename + ")");
                DownloadFile(filename);
                if (!File.Exists(Path.Combine("data", filename)))
                {
                    //Console.WriteLine("Download failed!");
                    return false;
                }
                else
                {
                    //Console.WriteLine("Downloaded " + filename);
                    return true;
                }
            }
            else
            {
                //Console.WriteLine("File was already present on disk!");
                return true;
            }
        }
    }
}
