using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Utils;

namespace WoWFormatLib.DBC
{
    public class DBCReader<T> where T : new()
    {
        private const uint headerSize = 20;
        private const uint DBCFmtSig = 0x43424457;  // WDBC

        public int recordCount { get; private set; }
        public int fieldCount { get; private set; }
        public int recordSize { get; private set; }
        public int stringBlockSize { get; private set; }

        private T[] m_rows;

        public T this[int row]
        {
            get { return m_rows[row]; }
        }

        public DBCReader(string filename)
        {
            if (!CASC.IsCASCInit)
                CASC.InitCasc();

            if (!CASC.FileExists(filename))
            {
                new MissingFile(filename);
                return;
            }

            using (var reader = new BinaryReader(new FileStream(Path.Combine("data", filename), FileMode.Open), Encoding.UTF8))
            {
                if (reader.BaseStream.Length < headerSize)
                {
                    throw new InvalidDataException(String.Format("File {0} is corrupted!", filename));
                }

                if (reader.ReadUInt32() != DBCFmtSig)
                {
                    throw new InvalidDataException(String.Format("File {0} isn't valid DBC file!", filename));
                }

                recordCount = reader.ReadInt32();
                fieldCount = reader.ReadInt32();
                recordSize = reader.ReadInt32();
                stringBlockSize = reader.ReadInt32();

                long pos = reader.BaseStream.Position;
                long stringTableStart = reader.BaseStream.Position + recordCount * recordSize;
                reader.BaseStream.Position = stringTableStart;

                Dictionary<int, string> StringTable = new Dictionary<int, string>();

                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    int index = (int)(reader.BaseStream.Position - stringTableStart);
                    StringTable[index] = reader.ReadStringNull();
                }

                reader.BaseStream.Position = pos;

                m_rows = new T[recordCount];

                var props = typeof(T).GetProperties();

                for (int i = 0; i < recordCount; i++)
                {
                    T row = new T();

                    long rowStart = reader.BaseStream.Position;

                    for (int j = 0; j < props.Length; j++)
                    {
                        switch (Type.GetTypeCode(props[j].PropertyType))
                        {
                            case TypeCode.Int32:
                                props[j].SetValue(row, reader.ReadInt32());
                                break;
                            case TypeCode.UInt32:
                                props[j].SetValue(row, reader.ReadUInt32());
                                break;
                            case TypeCode.Single:
                                props[j].SetValue(row, reader.ReadSingle());
                                break;
                            case TypeCode.String:
                                props[j].SetValue(row, StringTable[reader.ReadInt32()]);
                                break;
                            default:
                                throw new Exception("Unsupported field type " + Type.GetTypeCode(props[j].PropertyType));
                        }
                    }

                    if (reader.BaseStream.Position - rowStart != recordSize)
                    {
                        // struct bigger than record size
                        if (reader.BaseStream.Position - rowStart > recordSize)
                            throw new Exception("Incorrect DBC struct!");

                        // struct smaller than record size (imcomplete)
                        Console.WriteLine("Remaining data in row!");
                        int remaining = recordSize - (int)(reader.BaseStream.Position - rowStart);
                        reader.ReadBytes(remaining);
                    }

                    m_rows[i] = row;
                }
            }
        }
    }
}
