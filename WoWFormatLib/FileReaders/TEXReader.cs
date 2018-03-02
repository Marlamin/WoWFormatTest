using System;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Structs.TEX;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class TEXReader
    {
        public void LoadTEX(string filename)
        {
            if (CASC.cascHandler.FileExists(filename))
            {
                using (var tex = CASC.cascHandler.OpenFile(filename))
                {
                    ReadTEX(filename, tex);
                }
            }
        }
        private void ReadTEX(string filename, Stream tex)
        {
            var bin = new BinaryReader(tex);
            long position = 0;
            while (position < tex.Length)
            {
                tex.Position = position;

                var chunkName = (TEXChunks)bin.ReadUInt32();
                var chunkSize = bin.ReadUInt32();

                position = tex.Position + chunkSize;

                switch (chunkName)
                {
                    case TEXChunks.TXVR:
                        ReadTXVRChunk(bin);
                        break;
                    case TEXChunks.TXFN:
                        ReadTXFNChunk(bin, chunkSize);
                        break;
                    case TEXChunks.TXBT:
                    case TEXChunks.TXMD:
                        continue;
                    default:
                        throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position.ToString(), filename));
                }
            }
        }
        private void ReadTXFNChunk(BinaryReader bin, uint size)
        {
            //List of BLP filenames
            var blpFilesChunk = bin.ReadBytes((int)size);

            var str = new StringBuilder();

            for (var i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        str.Replace("..", ".");
                        str.Append(".blp"); //Filenames in TEX dont have have BLP extensions
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)blpFilesChunk[i]);
                }
            }
        }
        private void ReadTXVRChunk(BinaryReader bin)
        {
            if (bin.ReadUInt32() != 0)
            {
                throw new Exception("Unsupported TEX version!");
            }
        }
    }
}
