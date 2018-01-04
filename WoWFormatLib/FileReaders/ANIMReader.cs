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
            using (var bin = new BinaryReader(CASC.cascHandler.OpenFile(fileDataID)))
            {
                long position = 0;

                while (position < bin.BaseStream.Length)
                {
                    bin.BaseStream.Position = position;

                    var chunkName = new string(bin.ReadChars(4).ToArray());
                    var chunkSize = bin.ReadUInt32();

                    position = bin.BaseStream.Position + chunkSize;

                    switch (chunkName)
                    {
                        case "AFM2":
                        case "AFSA":
                        case "AFSB":
                            break;
                        default:
                            throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position, fileDataID));
                    }
                }
            }

        }
    }
}
