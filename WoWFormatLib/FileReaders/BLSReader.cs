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
            return LoadBLS(CASC.getFileDataIdByName(filename));
        }

        public BLS LoadBLS(int fileDataID)
        {
            if (CASC.cascHandler.FileExists(fileDataID))
            {
                using (var bin = new BinaryReader(CASC.cascHandler.OpenFile(fileDataID)))
                {
                    var identifier = new string(bin.ReadChars(4).Reverse().ToArray());
                    if(identifier != "GXSH")
                    {
                        throw new Exception("Unsupported shader file: " + identifier);
                    }

                    shaderFile.version = bin.ReadUInt32();

                    if(shaderFile.version != 0x10006)
                    {
                        Console.WriteLine("Unsupported shader version: " + shaderFile.version.ToString("X") + ", skipping..");
                        return shaderFile;
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

                        // Skip non-GL shaders for now
                        var magic = new string(bin.ReadChars(4));
                        if(magic != "3SLG")
                        {
                            Console.WriteLine("Skipping " + magic);
                            break; 
                        }
                        bin.BaseStream.Position -= 4;

                        var GLSL3start = bin.BaseStream.Position;

                        shaderFile.shaderBlocks[i].GLSL3Header = bin.Read<ShaderBlockHeader_GLSL3>();

                        var header = shaderFile.shaderBlocks[i].GLSL3Header;

                        if (bin.BaseStream.Position != (GLSL3start + header.codeOffset))
                        {
                            Console.WriteLine("!!! Didn't end up at codeOffset, header size might have changed " + bin.BaseStream.Position + "!");
                            bin.BaseStream.Position = GLSL3start + header.codeOffset;
                        }

                        shaderFile.shaderBlocks[i].shaderContent = bin.ReadCString();

                        if(bin.BaseStream.Position != (GLSL3start + header.inputParamsOffset))
                        {
                            Console.WriteLine("!!! Didn't end up at inputParamsOffset, code block might have changed " + bin.BaseStream.Position + "!");
                            bin.BaseStream.Position = GLSL3start + header.inputParamsOffset;
                        }

                        shaderFile.shaderBlocks[i].inputShaderInfo = new InputShaderInfo[header.inputParamCount];

                        for(var j = 0; j < header.inputParamCount; j++)
                        {
                            shaderFile.shaderBlocks[i].inputShaderInfo[j].glslParamNameOffset = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].inputShaderInfo[j].unk0 = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].inputShaderInfo[j].internalParamNameOffset = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].inputShaderInfo[j].unk1 = bin.ReadUInt32();

                            var prevPos = bin.BaseStream.Position;

                            bin.BaseStream.Position = GLSL3start + shaderFile.shaderBlocks[i].inputShaderInfo[j].glslParamNameOffset;
                            shaderFile.shaderBlocks[i].inputShaderInfo[j].glslParamName = bin.ReadCString();

                            bin.BaseStream.Position = GLSL3start + shaderFile.shaderBlocks[i].inputShaderInfo[j].internalParamNameOffset;
                            shaderFile.shaderBlocks[i].inputShaderInfo[j].internalParamName = bin.ReadCString();

                            bin.BaseStream.Position = prevPos;
                        }

                        if (bin.BaseStream.Position != (GLSL3start + header.outputOffset))
                        {
                            Console.WriteLine("!!! Didn't end up at outputOffset, input block might have changed " + bin.BaseStream.Position + "!");
                            bin.BaseStream.Position = GLSL3start + header.outputOffset;
                        }

                        shaderFile.shaderBlocks[i].outputShaderInfo = new OutputShaderInfo[header.outputCount];

                        for (var j = 0; j < header.outputCount; j++)
                        {
                            shaderFile.shaderBlocks[i].outputShaderInfo[j].glslParamNameOffset = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].outputShaderInfo[j].unk0 = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].outputShaderInfo[j].internalParamNameOffset = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].outputShaderInfo[j].unk1 = bin.ReadUInt32();

                            var prevPos = bin.BaseStream.Position;

                            bin.BaseStream.Position = GLSL3start + shaderFile.shaderBlocks[i].outputShaderInfo[j].glslParamNameOffset;
                            shaderFile.shaderBlocks[i].outputShaderInfo[j].glslParamName = bin.ReadCString();

                            bin.BaseStream.Position = GLSL3start + shaderFile.shaderBlocks[i].outputShaderInfo[j].internalParamNameOffset;
                            shaderFile.shaderBlocks[i].outputShaderInfo[j].internalParamName = bin.ReadCString();

                            bin.BaseStream.Position = prevPos;
                        }

                        if (bin.BaseStream.Position != (GLSL3start + header.uniformBufferOffset))
                        {
                            Console.WriteLine("!!! Didn't end up at uniformBufferOffset, output block might have changed " + bin.BaseStream.Position + "!");
                            bin.BaseStream.Position = GLSL3start + header.uniformBufferOffset;
                        }

                        shaderFile.shaderBlocks[i].uniformBufferInfo = new UniformBufferInfo[header.uniformBufferCount];

                        for (var j = 0; j < header.uniformBufferCount; j++)
                        {
                            shaderFile.shaderBlocks[i].uniformBufferInfo[j].glslParamNameOffset = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].uniformBufferInfo[j].unk0 = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].uniformBufferInfo[j].unk1 = bin.ReadUInt32();

                            var prevPos = bin.BaseStream.Position;

                            bin.BaseStream.Position = GLSL3start + shaderFile.shaderBlocks[i].uniformBufferInfo[j].glslParamNameOffset;
                            shaderFile.shaderBlocks[i].uniformBufferInfo[j].glslParamName = bin.ReadCString();

                            bin.BaseStream.Position = prevPos;
                        }

                        if (bin.BaseStream.Position != (GLSL3start + header.samplerUniformsOffset))
                        {
                            Console.WriteLine("!!! Didn't end up at samplerUniformsOffset, uniform block might have changed " + bin.BaseStream.Position + "!");
                            bin.BaseStream.Position = GLSL3start + header.samplerUniformsOffset;
                        }

                        shaderFile.shaderBlocks[i].sampleShaderInfo = new SamplerShaderInfo[header.samplerUniformsCount];

                        for (var j = 0; j < header.samplerUniformsCount; j++)
                        {
                            shaderFile.shaderBlocks[i].sampleShaderInfo[j].glslParamNameOffset = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].sampleShaderInfo[j].unk0 = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].sampleShaderInfo[j].unk1 = bin.ReadUInt32();
                            shaderFile.shaderBlocks[i].sampleShaderInfo[j].unk2 = bin.ReadUInt32();

                            var prevPos = bin.BaseStream.Position;

                            bin.BaseStream.Position = GLSL3start + shaderFile.shaderBlocks[i].sampleShaderInfo[j].glslParamNameOffset;
                            shaderFile.shaderBlocks[i].sampleShaderInfo[j].glslParamName = bin.ReadCString();

                            bin.BaseStream.Position = prevPos;
                        }

                        if (bin.BaseStream.Position != GLSL3start + header.variableStringsOffset)
                        {
                            Console.WriteLine("!!! Didn't end up at variable string block, sampler block might have changed " + bin.BaseStream.Position + "!");
                            bin.BaseStream.Position = GLSL3start + header.variableStringsOffset;
                        }

                        bin.BaseStream.Position += header.variableStringsSize;

                        // Padding up to 4 bytes
                        for (var p = 0; p < 4; p++)
                        {
                            if (bin.BaseStream.Position % 4 != 0 && bin.BaseStream.Position != bin.BaseStream.Length)
                            {
                                bin.ReadByte();
                            }
                        }

                        if (bin.BaseStream.Position != shaderFile.ofsShaderBlocks[i + 1])
                        {
                            Console.WriteLine("!!! Didn't end up at next shader (" + shaderFile.ofsShaderBlocks[i + 1] + "), there might be unread data at " + bin.BaseStream.Position + "!");
                        }
                    }
                }
            }
            else
            {
                throw new Exception("FileData ID " + fileDataID + " does not exist!");
            }

            return shaderFile;
        }
    }
}
