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

        public static int getFileDataIdByName(string name)
        {
            return (int)(cascHandler.Root as WowRootHandler)?.GetFileDataIdByName(name);
        }

        public static bool IsCASCInit { get { return fIsCASCInit; } }
    }
}
