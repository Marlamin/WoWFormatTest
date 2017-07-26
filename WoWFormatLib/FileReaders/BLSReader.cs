using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Structs.BLS;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class BLSReader
    {
        public BLS shaderFile;
        public BLS LoadBLS(string filename)
        {
            if(File.Exists("terrain.bls"))
            //if (CASC.cascHandler.FileExists(filename))
            {
                using (var bin = new BinaryReader(File.OpenRead("terrain.bls")))
                //using (var bin = new BinaryReader(CASC.cascHandler.OpenFile(filename)))
                {
                    var identifier = new string(bin.ReadChars(4).Reverse().ToArray());
                    if(identifier != "GXSH")
                    {
                        throw new Exception("Unsupported shader file: " + identifier);
                    }

                    shaderFile.version = bin.ReadUInt32();

                    if(shaderFile.version != 0x10004)
                    {
                        throw new Exception("Unsupported shader version: " + shaderFile.version);
                    }

                    shaderFile.permutationCount = bin.ReadUInt32();
                    shaderFile.nShaders = bin.ReadUInt32();
                    shaderFile.ofsCompressedChunks = bin.ReadUInt32();
                    shaderFile.nCompressedChunks = bin.ReadUInt32();
                    shaderFile.ofsCompressedData = bin.ReadUInt32();

                    // Lots of unks here, offsets into decompressed chunk?

                    bin.BaseStream.Position = shaderFile.ofsCompressedChunks;

                    var shaderOffsets = new uint[shaderFile.nCompressedChunks];
                    for (var i = 0; i < shaderFile.nCompressedChunks; i++)
                    {
                        shaderOffsets[i] = bin.ReadUInt32();
                    }

                    // 1 offset left?

                }
            }

            return shaderFile;
        }
    }
}
