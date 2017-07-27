using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        private MemoryStream targetStream;

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

                    shaderFile.ofsShaderBlocks = new uint[shaderFile.nShaders + 1];
                    for (var i = 0; i < (shaderFile.nShaders + 1); i++)
                    {
                        shaderFile.ofsShaderBlocks[i] = bin.ReadUInt32();
                    }

                    if(bin.BaseStream.Position != shaderFile.ofsCompressedChunks)
                    {
                        Console.WriteLine("!!! Didn't end up at ofsCompressedChunks, there might be unread data at " + bin.BaseStream.Position + "!");
                        bin.BaseStream.Position = shaderFile.ofsCompressedChunks;
                    }

                    var shaderOffsets = new uint[shaderFile.nCompressedChunks + 1];
                    for (var i = 0; i < (shaderFile.nCompressedChunks + 1); i++)
                    {
                        shaderOffsets[i] = bin.ReadUInt32();
                    }

                    targetStream = new MemoryStream();

                    for (var i = 0; i < shaderFile.nCompressedChunks; i++)
                    {
                        var chunkStart = shaderFile.ofsCompressedData + shaderOffsets[i];
                        var chunkLength = shaderOffsets[i + 1] - shaderOffsets[i];

                        bin.BaseStream.Position = chunkStart;

                        using (var compressed = new MemoryStream(bin.ReadBytes((int)chunkLength)))
                        {
                            // Skip zlib headers
                            compressed.ReadByte();
                            compressed.ReadByte();

                            using (var decompressionStream = new DeflateStream(compressed, CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(targetStream);
                            }
                        }
                    }
                }

                // Start reading decompressed data
                using (var bin = new BinaryReader(targetStream))
                {
                    shaderFile.shaderBlocks = new ShaderBlock[shaderFile.nShaders];

                    for (var i = 0; i < shaderFile.nShaders; i++)
                    {
                        var chunkLength = shaderFile.ofsShaderBlocks[i + 1] - shaderFile.ofsShaderBlocks[i];
                        bin.BaseStream.Position = shaderFile.ofsShaderBlocks[i];

                        shaderFile.shaderBlocks[i].header = bin.Read<ShaderBlockHeader>();
                        shaderFile.shaderBlocks[i].GLSL3Header = bin.Read<ShaderBlockHeader_GLSL3>();

                        shaderFile.shaderBlocks[i].shaderContent = bin.ReadStringNull();

                        // TODO: Read remaining Info Chunks
                    }
                }
            }

            return shaderFile;
        }
    }
}
