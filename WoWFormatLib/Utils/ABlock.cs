using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WoWFormatLib
{
    public struct ABlock<T> where T : struct
    {
        public ushort GlobalSequence;
        public ushort InterpolationType;
        public ArrayReference<ArrayReference<uint>> Timestamps;
        public ArrayReference<ArrayReference<T>> Values;
    }

    public struct ArrayReference<T> where T : struct
    {
        public uint Number;
        private uint elementsOffset;

        public IEnumerable<T> GetElements(BinaryReader bin)
        {
            var type = typeof(T);
            for (int i = 0; i < Number; i++)
            {
                var offset = elementsOffset + (i * Marshal.SizeOf(type));
                bin.BaseStream.Position += offset;
                yield return (T)bin.Read<T>();
            }
        }
    }

    public static class Extensions
    {
        public static T Read<T>(this BinaryReader bin)
        {
            var bytes = bin.ReadBytes(Marshal.SizeOf(typeof(T)));
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T ret = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return ret;
        }

        /// <summary>
        ///  Reads the NULL terminated string from the current stream and advances the current position of the stream by string length + 1.
        /// <seealso cref="GenericReader.ReadStringNumber"/>
        /// </summary>
        public static string ReadStringNull(this BinaryReader reader)
        {
            byte num;
            string text = String.Empty;
            System.Collections.Generic.List<byte> temp = new System.Collections.Generic.List<byte>();

            while ((num = reader.ReadByte()) != 0)
                temp.Add(num);

            text = Encoding.UTF8.GetString(temp.ToArray());

            return text;
        }

        /// <summary>
        ///  Reads the string with known length from the current stream and advances the current position of the stream by string length.
        /// <seealso cref="GenericReader.ReadStringNull"/>
        /// </summary>
        public static string ReadStringNumber(this BinaryReader reader)
        {
            string text = String.Empty;
            uint num = reader.ReadUInt32(); // string length

            for (uint i = 0; i < num; i++)
            {
                text += (char)reader.ReadByte();
            }
            return text;
        }

    }
}