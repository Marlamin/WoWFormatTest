using System;
using System.IO;
using System.Linq;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class ANIMReader
    {
        public void LoadAnim(string filename)
        {
            LoadAnim(CASC.getFileDataIdByName(Path.ChangeExtension(filename, "anim")));
        }

        public void LoadAnim(int fileDataID)
        {
            using (var anim = CASC.cascHandler.OpenFile(fileDataID))
            using (var bin = new BinaryReader(anim))
            {
                long position = 0;

                while (position < anim.Length)
                {
                    anim.Position = position;

                    var chunkName = new string(bin.ReadChars(4).ToArray());
                    var chunkSize = bin.ReadUInt32();

                    position = anim.Position + chunkSize;

                    switch (chunkName)
                    {
                        case "AFM2":
                        // These 2 have yet to be seen in files?
                        //case "AFSA":
                        //case "AFSB":
                            break;
                        default:
                            throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position, fileDataID));
                    }
                }
            }

        }
    }
}
