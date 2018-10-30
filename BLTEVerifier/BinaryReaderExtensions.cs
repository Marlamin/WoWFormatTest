using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.IO
{
    public unsafe struct MD5Hash
    {
        public fixed byte Value[16];
    }

    public static class CStringExtensions
    {
        /// <summary> Reads the NULL terminated string from 
        /// the current stream and advances the current position of the stream by string length + 1.
        /// <seealso cref="BinaryReader.ReadString"/>
        /// </summary>
        public static string ReadCString(this BinaryReader reader)
        {
            return reader.ReadCString(Encoding.UTF8);
        }

        /// <summary> Reads the NULL terminated string from 
        /// the current stream and advances the current position of the stream by string length + 1.
        /// <seealso cref="BinaryReader.ReadString"/>
        /// </summary>
        public static string ReadCString(this BinaryReader reader, Encoding encoding)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
                bytes.Add(b);
            return encoding.GetString(bytes.ToArray());
        }

        public static void WriteCString(this BinaryWriter writer, string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
            writer.Write((byte)0);
        }

        public static byte[] ToByteArray(this string str)
        {
            str = str.Replace(" ", string.Empty);

            var res = new byte[str.Length / 2];
            for (int i = 0; i < res.Length; ++i)
            {
                res[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }
            return res;
        }
    }

    public static class MD5HashExtensions
    {
        public static unsafe string ToHexString(this MD5Hash key)
        {
            byte[] array = new byte[16];

            fixed (byte* aptr = array)
            {
                *(MD5Hash*)aptr = key;
            }

            return array.ToHexString();
        }

        public static unsafe bool EqualsTo(this MD5Hash key, byte[] array)
        {
            if (array.Length != 16)
                return false;

            MD5Hash other;

            fixed (byte* ptr = array)
                other = *(MD5Hash*)ptr;

            for (int i = 0; i < 2; ++i)
            {
                ulong keyPart = *(ulong*)(key.Value + i * 8);
                ulong otherPart = *(ulong*)(other.Value + i * 8);

                if (keyPart != otherPart)
                    return false;
            }
            return true;
        }

        public static unsafe bool EqualsTo(this MD5Hash key, MD5Hash other)
        {
            for (int i = 0; i < 2; ++i)
            {
                ulong keyPart = *(ulong*)(key.Value + i * 8);
                ulong otherPart = *(ulong*)(other.Value + i * 8);

                if (keyPart != otherPart)
                    return false;
            }
            return true;
        }
    }

    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }

        public static unsafe MD5Hash ToMD5(this byte[] array)
        {
            if (array.Length != 16)
                throw new ArgumentException("array size != 16");

            fixed (byte* ptr = array)
            {
                return *(MD5Hash*)ptr;
            }
        }
    }

    public static class BinaryReaderExtensions
    {
        public static double ReadDouble(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToDouble(reader.ReadInvertedBytes(8), 0);
            }

            return reader.ReadDouble();
        }

        public static Int16 ReadInt16(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToInt16(reader.ReadInvertedBytes(2), 0);
            }

            return reader.ReadInt16();
        }

        public static Int32 ReadInt32(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToInt32(reader.ReadInvertedBytes(4), 0);
            }

            return reader.ReadInt32();
        }

        public static int ReadInt32BE(this BinaryReader reader)
        {
            byte[] val = reader.ReadBytes(4);
            return val[3] | val[2] << 8 | val[1] << 16 | val[0] << 24;
        }

        public static Int64 ReadInt64(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToInt64(reader.ReadInvertedBytes(8), 0);
            }

            return reader.ReadInt64();
        }

        public static Single ReadSingle(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToSingle(reader.ReadInvertedBytes(4), 0);
            }

            return reader.ReadSingle();
        }

        public static UInt16 ReadUInt16(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToUInt16(reader.ReadInvertedBytes(2), 0);
            }

            return reader.ReadUInt16();
        }

        public static UInt32 ReadUInt32(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToUInt32(reader.ReadInvertedBytes(4), 0);
            }

            return reader.ReadUInt32();
        }

        public static UInt64 ReadUInt64(this BinaryReader reader, bool invertEndian = false)
        {
            if (invertEndian)
            {
                return BitConverter.ToUInt64(reader.ReadInvertedBytes(8), 0);
            }

            return reader.ReadUInt64();
        }

        public static long ReadInt40BE(this BinaryReader reader)
        {
            byte[] val = reader.ReadBytes(5);
            return val[4] | val[3] << 8 | val[2] << 16 | val[1] << 24 | val[0] << 32;
        }

        public static UInt64 ReadUInt40(this BinaryReader reader, bool invertEndian = false)
        {
            ulong b1 = reader.ReadByte();
            ulong b2 = reader.ReadByte();
            ulong b3 = reader.ReadByte();
            ulong b4 = reader.ReadByte();
            ulong b5 = reader.ReadByte();

            if (invertEndian)
            {
                return (ulong)(b1 << 32 | b2 << 24 | b3 << 16 | b4 << 8 | b5);
            }
            else
            {
                return (ulong)(b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1);
            }
        }

        private static byte[] ReadInvertedBytes(this BinaryReader reader, int byteCount)
        {
            byte[] byteArray = reader.ReadBytes(byteCount);
            Array.Reverse(byteArray);

            return byteArray;
        }

        public static T Read<T>(this BinaryReader reader) where T : struct
        {
            byte[] result = reader.ReadBytes(Unsafe.SizeOf<T>());

            return Unsafe.ReadUnaligned<T>(ref result[0]);
        }
    }
}
