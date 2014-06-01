using System.IO;
using System;
using System.Linq;
using System.Runtime.InteropServices;
//using SharpDX.Toolkit.Content;
using System.Configuration;

namespace WoWRenderTest
{
    class MpqArchive
    {
        private const int MpqOpenReadOnly = 256;
        private static readonly IntPtr Archive;

        static MpqArchive()
        {

        }

        private const int SfileInfoFileSize = 105;

        static public Stream Open(string file)
        {
            var basedir = ConfigurationManager.AppSettings["basedir"];
            var filename = Path.Combine(basedir, file);
            if (File.Exists(Path.Combine(basedir, file)))
            {
                return File.Open(filename, FileMode.Open);
            }else{
                return null;
            }
            
        }
    }

    internal class MpqFile : BinaryReader
    {
        public MpqFile(Stream input) : base(input)
        {
        }

        public string ReadString(int length)
        {
            return new string(ReadChars(length));
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public T ReadStruct<T>() where T : struct
        {
            byte[] data = ReadBytes(Marshal.SizeOf(typeof (T)));

            if (data.Length < Marshal.SizeOf(typeof (T)))
                return default(T);

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            T ret = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (T));
            handle.Free();

            return ret;
        }

        public long GetChunkPosition(string id, long start)
        {
            Seek(start, SeekOrigin.Begin);
            var fcc = id.ToArray().Reverse();

            while (true)
            {
                var header = ReadStruct<ChunkHeader>();
                if (header.Size == 0)
                    break;

                if (header.Id.SequenceEqual(fcc))
                    return Position - 8;

                Seek(header.Size, SeekOrigin.Current);
            }
            throw new Exception();
            //return -1;
        }

        public long GetChunkPosition(string id)
        {
            return GetChunkPosition(id, 0);
        }

        public long Position
        {
            get { return BaseStream.Position; }
        }

        public long Length
        {
            get { return BaseStream.Length; }
        }
    }
}