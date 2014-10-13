using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CASCExplorer;
using System.IO;

namespace WoWFormatLib.Utils
{
    class CASC
    {
        public static CASCHandler cascHandler;
        private static LocaleFlags locale = LocaleFlags.All;
        private static ContentFlags content = ContentFlags.None;

        public static void InitCasc(){
            cascHandler = CASCHandler.OpenOnlineStorage("wow_beta");
        }

        public static void GenerateListfile()
        {
            //extract signaturefile and extract list of some files from that
            cascHandler.SaveFileTo("signaturefile", "data/", locale, content);
            FileStream stream = File.Open("data/signaturefile", FileMode.Open);
        }

        public static void DownloadFile()
        {
            cascHandler.SaveFileTo("signaturefile", "data/", locale, content);
        }
    }
}
