using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Utils;

namespace WoWFormatLib.DBC
{
    public class DB2Reader<T> where T : new()
    {
        public int recordCount { get; private set; }
        public int fieldCount { get; private set; }
        public int recordSize { get; private set; }
        public int stringTableSize { get; private set; }
        public int tablehash { get; private set; }
        public int build { get; private set; }
        public int timestamp_last_written { get; private set; }
        public int min_id { get; private set; }
        public int max_id { get; private set; }
        public int locale { get; private set; }
        public int copyTableSize { get; private set; }
        public int metaFlags { get; private set; }

        private T[] m_rows;

        private uint headerSize;

        public T this[int row]
        {
            get { return m_rows[row]; }
        }

        public DB2Reader(string filename)
        {
            if (!CASC.IsCASCInit)
                CASC.InitCasc();

            if (!CASC.FileExists(filename))
            {
                new MissingFile(filename);
                return;
            }

            using (var reader = new BinaryReader(CASC.OpenFile(filename)))
            {
                

                var magic = new string(reader.ReadChars(4));

                if (magic != "WDB2" && magic != "WDB4")
                {
                    throw new InvalidDataException(String.Format("File {0} isn't valid DB2/DB4 file!", filename));
                }

                if(magic == "WDB2")
                {
                    headerSize = 48;
                }
                else
                {
                    headerSize = 52;
                }

                if (reader.BaseStream.Length < headerSize)
                {
                    throw new InvalidDataException(String.Format("File {0} is corrupted!", filename));
                }
                 

                recordCount = reader.ReadInt32();
                fieldCount = reader.ReadInt32();
                recordSize = reader.ReadInt32();
                stringTableSize = reader.ReadInt32();
                tablehash = reader.ReadInt32();
                build = reader.ReadInt32();
                timestamp_last_written = reader.ReadInt32();
                min_id = reader.ReadInt32();
                max_id = reader.ReadInt32();
                locale = reader.ReadInt32();
                copyTableSize = reader.ReadInt32();

                if(magic == "WDB4")
                {
                    metaFlags = reader.ReadInt32();
                }

                long stringTableStart = reader.BaseStream.Position + recordCount * recordSize;
                long stringTableEnd = stringTableStart + stringTableSize;

                long pos = reader.BaseStream.Position;
                
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
                            throw new Exception("Incorrect DB2 struct!");

                        // struct smaller than record size (imcomplete)
                        //Console.WriteLine("Remaining data in row!");
                        int remaining = recordSize - (int)(reader.BaseStream.Position - rowStart);
                        reader.ReadBytes(remaining);
                    }

                    m_rows[i] = row;
                }
            }
        }
    }
}
