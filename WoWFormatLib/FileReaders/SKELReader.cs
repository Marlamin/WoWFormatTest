using System;
using System.IO;
using WoWFormatLib.Structs.SKIN;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class SKELReader
    {
        public void LoadSKEL(int fileDataID)
        {
            using (var bin = new BinaryReader(CASC.cascHandler.OpenFile(fileDataID)))
            {
                var header = new string(bin.ReadChars(4));
              
            }
        }
    }
}
