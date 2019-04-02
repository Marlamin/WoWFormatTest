using System.IO;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class SKELReader
    {
        public void LoadSKEL(uint fileDataID)
        {
            using (var bin = new BinaryReader(CASC.OpenFile(fileDataID)))
            {
                var header = new string(bin.ReadChars(4));

            }
        }
    }
}
