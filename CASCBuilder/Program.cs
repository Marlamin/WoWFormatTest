using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CASCBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderToCASCify = @"G:\WoW\0.5.3.3368 [Start] [A]\Client";

            Console.Write("Listing files..");
            List<string> files = Directory.GetFiles(folderToCASCify, "*", SearchOption.AllDirectories).ToList();
            Console.Write("..done (" + files.Count() + " files)!\n");

            var encodingEntries = new EncodingFileEntry[files.Count()];
            Console.Write("Generating file array..");
            using (var stream = new MemoryStream())
            {
                File.OpenRead("H:/tpr/wow/data/00/6d/006dd8df4c7cd10a2b6b319a7e2abe37").CopyTo(stream);
                var result = ParseBLTEfile(stream.ToArray());
                var hasher = MD5.Create();
                var md5sum = hasher.ComputeHash(result);
                Console.WriteLine(BitConverter.ToString(md5sum).Replace("-", ""));
            }
            Console.Write("..done!\n");

            using (BinaryWriter writer = new BinaryWriter(File.Open("encoding_decoded", FileMode.Create)))
            {
                writer.Write(new char[] { 'E', 'N' });  // Signature
                writer.Write(new byte[] { 1 });         // Unknown byte
                writer.Write(new byte[] { 16 });        // Checksum Size A
                writer.Write(new byte[] { 16 });        // Checksum Size B
                writer.Write((short)1024);              // Flags A
                writer.Write((short)1024);              // Flags B
                writer.Write(BitConverter.GetBytes(files.Count()).Reverse().ToArray()); // Table A entries
                writer.Write((uint)0);                  // Table B entries
                writer.Write(new byte[] { 0 });         // Unknown byte
                writer.Write((uint)0);                  // String block size

                for (var i = 0; i < files.Count(); i++)
                {
                    writer.Write(new byte[16] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF });
                    writer.Write(new byte[16] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF });
                }

                var numBlocks = (files.Count() * 38) / 4096;
                var fileCounter = 0;
                for(var i = 0; i < numBlocks; i++)
                {
                    var startPos = writer.BaseStream.Position;

                    for (var j = 0; j < 107; j++)
                    {
                        writer.Write((short)1);
                    }

                }
                Console.WriteLine("Need to write " + numBlocks + " blocks to fit all entries!"); 
            }

            File.WriteAllBytes("encoding_encoded", MakeBlteFile(File.ReadAllBytes("encoding_decoded")));

            Console.ReadLine();
        }

        public struct EncodingFileEntry
        {
            public ushort keyCount;
            public uint size;
            public string hash;
            public string key;
        }

        public static byte[] MakeBlteFile(byte[] contents)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(new char[] { 'B', 'L', 'T', 'E' });
                writer.Write((int)0);
                writer.Write('N');
                writer.Write(contents);
                return stream.ToArray();
            }
        }

        public struct BLTEChunkInfo
        {
            public bool isFullChunk;
            public int inFileSize;
            public int actualSize;
            public byte[] checkSum;
        }

        public static byte[] MakeBigEndian(byte[] contents)
        {
            return contents.Reverse().ToArray();
        }

        private static byte[] ParseBLTEfile(byte[] content)
        {
            MemoryStream result = new MemoryStream();

            using (BinaryReader bin = new BinaryReader(new MemoryStream(content)))
            {
                if (bin.ReadUInt32() != 0x45544c42) { throw new Exception("Not a BLTE file"); }

                var blteSize = bin.ReadUInt32(true);

                BLTEChunkInfo[] chunkInfos;

                if (blteSize == 0)
                {
                    chunkInfos = new BLTEChunkInfo[1];
                    chunkInfos[0].isFullChunk = false;
                    chunkInfos[0].inFileSize = Convert.ToInt32(bin.BaseStream.Length - bin.BaseStream.Position);
                    chunkInfos[0].actualSize = Convert.ToInt32(bin.BaseStream.Length - bin.BaseStream.Position);
                    chunkInfos[0].checkSum = new byte[16]; ;
                }
                else
                {

                    var bytes = bin.ReadBytes(4);

                    var chunkCount = bytes[1] << 16 | bytes[2] << 8 | bytes[3] << 0;

                    //var unk = bin.ReadByte();

                    ////Code by TOM_RUS 
                    //byte v1 = bin.ReadByte();
                    //byte v2 = bin.ReadByte();
                    //byte v3 = bin.ReadByte();
                    //var chunkCount = v1 << 16 | v2 << 8 | v3 << 0; // 3-byte
                    ////Retrieved from https://github.com/WoW-Tools/CASCExplorer/blob/cli/CascLib/BLTEHandler.cs#L76

                    var supposedHeaderSize = 24 * chunkCount + 12;

                    if (supposedHeaderSize != blteSize)
                    {
                        throw new Exception("Invalid header size!");
                    }

                    if (supposedHeaderSize > bin.BaseStream.Length)
                    {
                        throw new Exception("Not enough data");
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
                }

                foreach (var chunk in chunkInfos)
                {
                    MemoryStream chunkResult = new MemoryStream();

                    if (chunk.inFileSize > bin.BaseStream.Length)
                    {
                        throw new Exception("Trying to read more than is available!");
                    }

                    var chunkBuffer = bin.ReadBytes(chunk.inFileSize);

                    var hasher = MD5.Create();
                    var md5sum = hasher.ComputeHash(chunkBuffer);

                    if (chunk.isFullChunk && BitConverter.ToString(md5sum) != BitConverter.ToString(chunk.checkSum))
                    {
                        throw new Exception("MD5 checksum mismatch on BLTE chunk! Sum is " + BitConverter.ToString(md5sum).Replace("-", "") + " but is supposed to be " + BitConverter.ToString(chunk.checkSum).Replace("-", ""));
                    }

                    using (BinaryReader chunkreader = new BinaryReader(new MemoryStream(chunkBuffer)))
                    {
                        var mode = chunkreader.ReadChar();
                        switch (mode)
                        {
                            case 'N': // none
                                chunkResult.Write(chunkreader.ReadBytes(chunk.actualSize), 0, chunk.actualSize); //read actual size because we already read the N from chunkreader
                                break;
                            case 'Z': // zlib, todo
                                using (MemoryStream stream = new MemoryStream(chunkreader.ReadBytes(chunk.inFileSize - 1), 2, chunk.inFileSize - 3))
                                {
                                    var ds = new DeflateStream(stream, CompressionMode.Decompress);
                                    ds.CopyTo(chunkResult);
                                }
                                break;
                            case 'F': // frame
                            case 'E': // encrypted
                                Console.WriteLine("Encrypted file!");
                                break;
                            default:
                                throw new Exception("Unsupported mode!");
                        }

                        if (mode == 'N' || mode == 'Z')
                        {
                            var chunkres = chunkResult.ToArray();
                            if (chunk.isFullChunk && chunkres.Length != chunk.actualSize)
                            {
                                throw new Exception("Decoded result is wrong size!");
                            }

                            result.Write(chunkres, 0, chunkres.Length);
                        }
                        else
                        {
                            Console.WriteLine("Unsupported file (Encrypted?)");
                        }
                    }
                }

                foreach (var chunk in chunkInfos)
                {
                    if (chunk.inFileSize > bin.BaseStream.Length)
                    {
                        throw new Exception("Trying to read more than is available!");
                    }
                    else
                    {
                        bin.BaseStream.Position += chunk.inFileSize;
                    }
                }
            }

            return result.ToArray();
        }
    }
}
