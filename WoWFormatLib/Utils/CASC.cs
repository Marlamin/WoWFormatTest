﻿using System;
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
        private static LocaleFlags locale = LocaleFlags.All;
        private static AsyncAction bgAction;
        public static int progressNum;
        public static string progressDesc;

        public static void InitCasc(AsyncAction bgAction = null){
            CASC.bgAction = bgAction;
            //bgAction.ProgressChanged += new EventHandler<AsyncActionProgressChangedEventArgs>(bgAction_ProgressChanged);
            cascHandler = CASCHandler.OpenOnlineStorage("wow_beta", bgAction);
        }

        private static void bgAction_ProgressChanged(object sender, AsyncActionProgressChangedEventArgs progress)
        {
            if (bgAction.IsCancellationRequested) { return; }
            progressNum = progress.Progress;
            if (progress.UserData != null) { progressDesc = progress.UserData.ToString(); }
        }

        public static void GenerateListfile()
        {
            //extract signaturefile and extract list of some files from that
            cascHandler.SaveFileTo("signaturefile", "data/", locale);
            string line;
            string[] linesplit;
            List<string> files = new List<String>();

            System.IO.StreamReader file = new System.IO.StreamReader("data/signaturefile");

            File.WriteAllText("listfile.txt", "");
            while ((line = file.ReadLine()) != null)
            {
                linesplit = line.Split(';');
                if (linesplit.Count() != 4) { continue; } //filter out junk 
                
                if (linesplit[3].EndsWith(".wmo", StringComparison.OrdinalIgnoreCase) || linesplit[3].EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(linesplit[3]);
                }

            }
            File.AppendAllLines("listfile.txt", files.ToArray());
        }

        public static void DownloadFile(string filename)
        {
            if (!File.Exists(Path.Combine("data", filename)))
            {
                cascHandler.SaveFileTo(filename, "data/", locale);
                Console.WriteLine("Downloaded " + filename + "!");
            }
            else
            {
                cascHandler.SaveFileTo(filename, "data/", locale);
                Console.WriteLine(filename + " was already present on disk!");
            }
            
        }
    }
}
