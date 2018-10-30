using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
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

        public class IndexEntry
        {
            public int Index;
            public int Offset;
            public int Size;
        }

        public static unsafe IndexEntry GetIndexInfo(MD5Hash key)
        {
            ulong* ptr = (ulong*)&key;
            ptr[1] &= 0xFF;

            if (!LocalIndexData.TryGetValue(key, out IndexEntry result))
                Console.WriteLine("LocalIndexHandler: missing index: {0}", key.ToHexString());

            return result;
        }

        private static readonly MD5HashComparer comparer = new MD5HashComparer();
        private static Dictionary<MD5Hash, IndexEntry> LocalIndexData = new Dictionary<MD5Hash, IndexEntry>(comparer);

        static unsafe void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                throw new ArgumentOutOfRangeException("Program requires at least one argument: directory that it will need to verify all contents of and optionally a location of local indexes/archives");
            }

            if(args.Count() > 1)
            {
                foreach (var file in Directory.GetFiles(args[1], "*.idx", SearchOption.AllDirectories))
                {
                    using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var br = new BinaryReader(stream))
                    {
                        int h2Len = br.ReadInt32();
                        int h2Check = br.ReadInt32();
                        byte[] h2 = br.ReadBytes(h2Len);

                        long padPos = (8 + h2Len + 0x0F) & 0xFFFFFFF0;
                        stream.Position = padPos;

                        int dataLen = br.ReadInt32();
                        int dataCheck = br.ReadInt32();

                        int numBlocks = dataLen / 18;

                        for (int i = 0; i < numBlocks; i++)
                        {
                            var info = new IndexEntry();
                            byte[] keyBytes = br.ReadBytes(9);
                            Array.Resize(ref keyBytes, 16);

                            MD5Hash key;

                            fixed (byte* ptr = keyBytes)
                                key = *(MD5Hash*)ptr;

                            //Console.WriteLine(key.ToHexString());

                            byte indexHigh = br.ReadByte();
                            int indexLow = br.ReadInt32BE();

                            info.Index = (indexHigh << 2 | (byte)((indexLow & 0xC0000000) >> 30));
                            info.Offset = (indexLow & 0x3FFFFFFF);
                            info.Size = br.ReadInt32();

                            if (!LocalIndexData.ContainsKey(key)) // use first key
                                LocalIndexData.Add(key, info);
                        }

                        padPos = (dataLen + 0x0FFF) & 0xFFFFF000;
                        stream.Position = padPos;

                        stream.Position += numBlocks * 18;
                    }
                }
            }

            Console.WriteLine("Reading all archives on disk..");
            var archiveList = new List<MD5Hash>();

            foreach (var file in Directory.GetFiles(Path.Combine(args[0], "tpr", "wow", "data"), "*.index", SearchOption.AllDirectories))
            {
                var indexName = Path.GetFileNameWithoutExtension(file);
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var bin = new BinaryReader(stream))
                {
                    bin.BaseStream.Position = bin.BaseStream.Length - 16;
                    // 0 = loose file index, 4 = archive index, 6 = group archive index
                    if (bin.ReadChar() == 4)
                    {
                        if (!File.Exists(Path.Combine(args[0], "tpr", "wow", "patch", "" + indexName[0] + indexName[1], "" + indexName[2] + indexName[3], indexName + ".index")))
                        {
                            if (File.Exists(Path.Combine(args[0], "tpr", "wow", "data", "" + indexName[0] + indexName[1], "" + indexName[2] + indexName[3], indexName)))
                            {
                                archiveList.Add(Path.GetFileNameWithoutExtension(file).ToByteArray().ToMD5());
                            }
                        }
                    }
                }
            }

            //18125-18179 missing
            var missingArchiveList = new List<MD5Hash>
            {
                "5BEBD82DD19DEFD0A0BB2B97C178A59F".ToByteArray().ToMD5(),
                "5E430F0C8A3F77B2E6F62F4BA74E1147".ToByteArray().ToMD5(),
                "6F75F753E8CF156FD5B7381F9A63C210".ToByteArray().ToMD5(),
                "9A8618C3393299DA9BDAD3649A0B7E5D".ToByteArray().ToMD5(),
                "9F02F9518B4844B85D957A6371C98656".ToByteArray().ToMD5(),
                "F9B96A5557541391ED3770B85B06D5A8".ToByteArray().ToMD5()
            };

            Console.WriteLine("Checking all archives for files for missing archives..");

            Parallel.ForEach(archiveList.ToArray(), (archive, state, i) =>
            {
                var indexName = archive.ToHexString().ToLower();

                var indexContent = File.ReadAllBytes(Path.Combine(args[0], "tpr", "wow", "data", "" + indexName[0] + indexName[1], "" + indexName[2] + indexName[3], indexName + ".index"));

                var foundCount = 0;
                var totalCount = 0;
                
                using (var indexBin = new BinaryReader(new MemoryStream(indexContent)))
                {
                    var indexEntries = indexContent.Length / 4096;

                    FileStream stream;
                    var archiveBin = new BinaryReader(new MemoryStream());

                    if (!missingArchiveList.Contains(archive))
                    {
                        stream = new FileStream(Path.Combine(args[0], "tpr", "wow", "data", "" + indexName[0] + indexName[1], "" + indexName[2] + indexName[3], indexName), FileMode.Open, FileAccess.Read, FileShare.Read);
                        archiveBin = new BinaryReader(stream);
                    }

                    for (var b = 0; b < indexEntries; b++)
                    {
                        for (var bi = 0; bi < 170; bi++)
                        {
                            var headerHash = indexBin.Read<MD5Hash>();
                            var size = indexBin.ReadUInt32(true);
                            var offset = indexBin.ReadUInt32(true);

                            if (headerHash.ToHexString() == "00000000000000000000000000000000")
                                continue;

                            if (!missingArchiveList.Contains(archive))
                            {
                                if (offset > archiveBin.BaseStream.Length)
                                {
                                    Console.WriteLine("[" + indexName + "] Unable to read hash " + headerHash.ToHexString() + ", offset " + offset + " is after end of stream " + archiveBin.BaseStream.Length + "!");
                                }
                                else if ((offset + size) > archiveBin.BaseStream.Length)
                                {
                                    Console.WriteLine("[" + indexName + "] Unable to read hash " + headerHash.ToHexString() + ", offset " + offset + "+" + size + " goes on beyond end of stream " + archiveBin.BaseStream.Length + "!");
                                }
                                else
                                {
                                    archiveBin.BaseStream.Position = offset;
                                    if(archiveBin.ReadUInt64() == 0)
                                    {
                                        Console.WriteLine("[" + indexName + "] Unable to read hash " + headerHash.ToHexString() + ", offset " + offset + "+" + size + ", it starts with all 0s!");
                                    }
                                }
                            }
                            else
                            {
                                var indexEntry = GetIndexInfo(headerHash);
                                if (indexEntry != null)
                                {
                                    foundCount++;
                                    Console.WriteLine("[" + indexName + "] Unable to read hash " + headerHash.ToHexString() + ", offset " + offset + " size " + size + " since archive does not exist but file exists in local archives in archive " + indexEntry.Index + " at offset " + indexEntry.Offset + " with size " + indexEntry.Size + "!");
                                }
                                else
                                {
                                    Console.WriteLine("[" + indexName + "] Unable to read hash " + headerHash.ToHexString() + ", offset " + offset + " archive does not exist!");
                                }
                                totalCount++;
                            }
                        }
                        indexBin.ReadBytes(16);
                    }

                    if (missingArchiveList.Contains(archive))
                    {
                        Console.WriteLine("Found " + foundCount + " of " + totalCount + " files for archive " + indexName + " in local archives!");
                    }
                }
            });

            Environment.Exit(0);
            //GetIndexes(Path.Combine(CDN.cacheDir, "tpr", "wow"), archiveList.ToArray());

            Console.WriteLine("Listing directory..");

            var files = new string[0];

            // Hackfix to read from txt file instead
            if (args[0].EndsWith(".txt"))
            {
                files = File.ReadAllLines(args[0]);
            }
            else
            {
                files = Directory.GetFiles(Path.Combine(args[0], "tpr", "wow"), "*", SearchOption.AllDirectories);
            }

            foreach (var file in files)
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
                            if (header == 0x4453425A)
                            {
                                Console.WriteLine(" File is a patch archive!");
                            }
                            else if (header == 268583248)
                            {
                                Console.WriteLine(" File is a patch index!");
                            }
                            else if (header == 893668643 || header == 1279870499)
                            {
                                Console.WriteLine(" File is Overwatch root file!");
                            }
                            else
                            {
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

                                for (var i = 0; i < chunkCount; i++)
                                {
                                    chunkInfos[i].isFullChunk = true;
                                    chunkInfos[i].inFileSize = bin.ReadInt32(true);
                                    chunkInfos[i].actualSize = bin.ReadInt32(true);
                                    chunkInfos[i].checkSum = new byte[16];
                                    chunkInfos[i].checkSum = bin.ReadBytes(16);
                                }

                                foreach (var chunk in chunkInfos)
                                {
                                    using (var chunkResult = new MemoryStream())
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

                                        using (var chunkms = new MemoryStream(chunkBuffer))
                                        using (var chunkreader = new BinaryReader(chunkms))
                                        {
                                            var mode = chunkreader.ReadChar();
                                            switch (mode)
                                            {
                                                case 'N': // none
                                                    chunkResult.Write(chunkreader.ReadBytes(chunk.actualSize), 0, chunk.actualSize); //read actual size because we already read the N from chunkreader
                                                    break;
                                                case 'Z': // zlib, todo
                                                    using (var mstream = new MemoryStream(chunkreader.ReadBytes(chunk.inFileSize - 1), 2, chunk.inFileSize - 3))
                                                    using (var ds = new DeflateStream(mstream, CompressionMode.Decompress))
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
                                            if (mode == 'N' || mode == 'Z')
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
