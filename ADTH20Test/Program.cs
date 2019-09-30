using System;
using System.ComponentModel;
using CASCLib;
using WoWFormatLib.Utils;

namespace ADTH20Test
{
    class Program
    {
        private static BackgroundWorkerEx cascworker = new BackgroundWorkerEx();

        static void Main(string[] args)
        {
            cascworker.RunWorkerCompleted += CASCworker_RunWorkerCompleted;
            cascworker.ProgressChanged += CASC_ProgressChanged;
            cascworker.WorkerReportsProgress = true;

            CASC.InitCasc(cascworker, "C:\\Program Files (x86)\\World of Warcraft", "wow");

            ADTExporter.ExportADT(775971, 33, 32);
        }

        private static void CASC_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine((string)e.UserState);
        }

        private static void CASCworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Done initializing CASC!");
        }
    }
}
