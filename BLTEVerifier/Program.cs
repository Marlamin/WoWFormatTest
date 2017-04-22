using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BLTEVerifier
{
    class Program
    {
        public struct BLTEChunkInfo
        {
            public bool isFullChunk;
            public int inFileSize;
            public int actualSize;
            public byte[] checkSum;
            public char mode;
        }

        static void Main(string[] args)
        {
            if(args.Count() != 1)
            {
                throw new ArgumentOutOfRangeException("Program requires one argument: directory that it will need to verify all contents of");
            }

            Console.WriteLine("Listing directory..");

            string[] files = new string[0];

            // Hackfix to read from txt file instead
            if (args[0].EndsWith(".txt"))
            {
                files = File.ReadAllLines(args[0]);
            }
            else
            {
                files = Directory.GetFiles(args[0], "*", SearchOption.AllDirectories);
            }

            foreach (string file in files)
            {
                Console.WriteLine(file);

                if (file.Contains("config"))
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(file))
                        {
                            var md5sum = BitConverter.ToString(md5.ComputeHash(stream)).ToLower().Replace("-", "");
                            if (md5sum != Path.GetFileName(file))
                            {
                                throw new Exception("   Invalid config file! MD5 of " + Path.GetFileName(file) + " is " + md5sum);
                            }
                        }
                    }
                    continue;
                }

                if (file.EndsWith(".index"))
                {
                    //Console.WriteLine(" Not checking indexes");
                    continue;
                }

                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var bin = new BinaryReader(stream))
                {
                    try
                    {
                        var header = bin.ReadUInt32();
                        if (header != 0x45544C42)
                        {
                            Console.WriteLine(" Invalid BLTE file! " + header);
                            if(header == 0x4453425A)
                            {
                                Console.WriteLine(" File is a patch archive!");
                            }else if(header == 268583248){
                                Console.WriteLine(" File is a patch index!");
                            }else if(header == 893668643 || header == 1279870499)
                            {
                                Console.WriteLine(" File is Overwatch root file!");
                            }else{
                                Console.WriteLine("Unknown file encountered!");
                                Console.ReadLine();
                            }
                        }
                        else
                        {
                            var blteSize = bin.ReadUInt32(true);

                            BLTEChunkInfo[] chunkInfos;

                            if (blteSize == 0)
                            {
                                Console.WriteLine(file + " is a single chunk file!");
                                chunkInfos = new BLTEChunkInfo[1];
                                chunkInfos[0].isFullChunk = false;
                                chunkInfos[0].inFileSize = Convert.ToInt32(bin.BaseStream.Length - bin.BaseStream.Position);
                                chunkInfos[0].actualSize = Convert.ToInt32(bin.BaseStream.Length - bin.BaseStream.Position);
                                chunkInfos[0].checkSum = new byte[16];
                            }
                            else
                            {

                                var bytes = bin.ReadBytes(4);

                                //Code by TOM_RUS 
                                //Retrieved from https://github.com/WoW-Tools/CASCExplorer/blob/cli/CascLib/BLTEHandler.cs#L76

                                var chunkCount = bytes[1] << 16 | bytes[2] << 8 | bytes[3] << 0;

                                var supposedHeaderSize = 24 * chunkCount + 12;

                                if (supposedHeaderSize != blteSize)
                                {
                                    File.AppendAllText("bad.txt", file + " (Invalid header size) " + Environment.NewLine);
                                }

                                if (supposedHeaderSize > bin.BaseStream.Length)
                                {
                                    File.AppendAllText("bad.txt", file + " (Not enough data) " + Environment.NewLine);
                                }

                                chunkInfos = new BLTEChunkInfo[chunkCount];

                                for (int i = 0; i < chunkCount; i++)
                                {
                                    chunkInfos[i].isFullChunk = true;
                                    chunkInfos[i].inFileSize = bin.ReadInt32(true);
                                    chunkInfos[i].actualSize = bin.ReadInt32(true);
                                    chunkInfos[i].checkSum = new byte[16];
                                    chunkInfos[i].checkSum = bin.ReadBytes(16);
                                }

                                foreach (var chunk in chunkInfos)
                                {
                                    using (MemoryStream chunkResult = new MemoryStream())
                                    {
                                        if (chunk.inFileSize > bin.BaseStream.Length)
                                        {
                                            File.AppendAllText("bad.txt", file + " (Not enough data remaining in stream) " + Environment.NewLine);
                                            continue;
                                        }

                                        var chunkBuffer = bin.ReadBytes(chunk.inFileSize);

                                        using (var hasher = MD5.Create())
                                        {
                                            var md5sum = hasher.ComputeHash(chunkBuffer);

                                            if (chunk.isFullChunk && BitConverter.ToString(md5sum) != BitConverter.ToString(chunk.checkSum))
                                            {
                                                File.AppendAllText("bad.txt", file + " (chunk md5sum mismatch) " + Environment.NewLine);
                                                chunkBuffer = null;
                                                continue;
                                            }
                                        }

                                        using (MemoryStream chunkms = new MemoryStream(chunkBuffer))
                                        using (BinaryReader chunkreader = new BinaryReader(chunkms))
                                        {
                                            var mode = chunkreader.ReadChar();
                                            switch (mode)
                                            {
                                                case 'N': // none
                                                    chunkResult.Write(chunkreader.ReadBytes(chunk.actualSize), 0, chunk.actualSize); //read actual size because we already read the N from chunkreader
                                                    break;
                                                case 'Z': // zlib, todo
                                                    using (MemoryStream mstream = new MemoryStream(chunkreader.ReadBytes(chunk.inFileSize - 1), 2, chunk.inFileSize - 3))
                                                    using (DeflateStream ds = new DeflateStream(mstream, CompressionMode.Decompress))
                                                    {
                                                        ds.CopyTo(chunkResult);
                                                    }
                                                    break;
                                                case 'E': // encrypted
                                                    //Console.WriteLine("Encrypted file!");
                                                    break;
                                                case 'F': // frame
                                                default:
                                                    throw new Exception("Unsupported mode!");
                                            }

                                            // Don't check integrity for unsupported chunks
                                            if(mode == 'N' || mode == 'Z')
                                            {
                                                var chunkres = chunkResult.ToArray();
                                                if (chunk.isFullChunk && chunkres.Length != chunk.actualSize)
                                                {
                                                    File.AppendAllText("bad.txt", file + " (bad chunk result size) " + Environment.NewLine);
                                                }
                                                chunkres = null;
                                            }
                                            chunkBuffer = null;
                                        }
                                    }
                                }
                                chunkInfos = null;
                            }
                        }
                    }
                    catch (EndOfStreamException e)
                    {
                        File.AppendAllText("bad.txt", file + Environment.NewLine);
                    }
                }
            }
        }
    }
}
