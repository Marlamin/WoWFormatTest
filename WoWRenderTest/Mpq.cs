using System.IO;
using System;
using System.Linq;
using System.Runtime.InteropServices;
//using SharpDX.Toolkit.Content;

namespace WoWRenderTest
{
    class MpqArchive
    {
        private const int MpqOpenReadOnly = 256;
        private static readonly IntPtr Archive;

        static MpqArchive()
        {
            const string path = @"E:\Wow 3.3.5a\World of Warcraft\Data\";
            SFileOpenArchive(path + "common.MPQ", 0, MpqOpenReadOnly, out Archive);
            string[] patches = { "common-2.MPQ", "expansion.MPQ", "patch.MPQ", "patch-2.MPQ", "patch-3.MPQ" };

            foreach (string s in patches)
                SFileOpenPatchArchive(Archive, path + s, "", 0);
        }

        private const int SfileInfoFileSize = 105;

        static public Stream Open(string file)
        {
            IntPtr handle;
            SFileOpenFileEx(Archive, file, 0, out handle);
            
            int read;
            int size;
            int needed;

            SFileGetFileInfo(handle, SfileInfoFileSize, out size, 4, out needed);

            var buffer = new byte[size];

            SFileReadFile(handle, buffer, size, out read, IntPtr.Zero);

            return new MemoryStream(buffer);
        }

        [DllImport("StormLib.dll")]
        static extern bool SFileOpenFileEx(IntPtr handle, string name, uint searchScope, out IntPtr file);

        [DllImport("StormLib.dll")]
        static extern bool SFileReadFile(IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int count, out int read, IntPtr io);

        [DllImport("StormLib.dll")]
        static extern bool SFileOpenArchive([MarshalAs(UnmanagedType.LPStr)] string name, int p, uint flags, out IntPtr handle);

        [DllImport("StormLib.dll")]
        static extern bool SFileOpenPatchArchive(IntPtr handle, string name, string prefix, uint flags);

        [DllImport("StormLib.dll")]
        private static extern bool SFileGetFileInfo(IntPtr hMpqOrFile, int dwInfoType, out int pvFileInfo, int cbFileInfo, out int pcbLengthNeeded);
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