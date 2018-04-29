/* This file was originally authored by TOM_RUS for CASCExplorer and is used with permission */
/* https://github.com/WoW-Tools/CASCExplorer/tree/master/CASCExplorer */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WoWFormatLib.DBC
{
    public class WDC2Row
    {
        private BitReader m_data;
        private WDC2Reader m_reader;
        private int Offset;
        private long m_recordsOffset;

        public uint Id { get; set; }

        private FieldMetaData[] fieldMeta;
        private ColumnMetaData[] columnMeta;
        private Value32[][] palletData;
        private Dictionary<uint, Value32>[] commonData;
        private ReferenceEntry? refData;

        public WDC2Row(WDC2Reader reader, BitReader data, long recordsOffset, uint id, ReferenceEntry? refData)
        {
            m_reader = reader;
            m_data = data;
            m_recordsOffset = recordsOffset;

            Offset = m_data.Offset;

            fieldMeta = reader.Meta;
            columnMeta = reader.ColumnMeta;
            palletData = reader.PalletData;
            commonData = reader.CommonData;
            this.refData = refData;

            if (id != 0xFFFFFFFF)
                Id = id;
            else
            {
                int idFieldIndex = reader.IdFieldIndex;

                m_data.Position = columnMeta[idFieldIndex].RecordOffset;
                m_data.Offset = Offset;

                Id = GetFieldValue<uint>(0, m_data, fieldMeta[idFieldIndex], columnMeta[idFieldIndex], palletData[idFieldIndex], commonData[idFieldIndex]);
            }
        }

        private static Dictionary<Type, Func<uint, BitReader, long, FieldMetaData, ColumnMetaData, Value32[], Dictionary<uint, Value32>, Dictionary<long, string>, object>> simpleReaders = new Dictionary<Type, Func<uint, BitReader, long, FieldMetaData, ColumnMetaData, Value32[], Dictionary<uint, Value32>, Dictionary<long, string>, object>>
        {
            [typeof(float)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValue<float>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(int)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(uint)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValue<uint>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(short)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValue<short>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(ushort)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValue<ushort>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(sbyte)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValue<sbyte>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(byte)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => GetFieldValue<byte>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(string)] = (id, data, recordsOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => { var pos = recordsOffset + data.Offset + (data.Position >> 3); int strOfs = GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData); return stringTable[pos + strOfs]; },
        };

        private static Dictionary<Type, Func<BitReader, FieldMetaData, ColumnMetaData, Value32[], Dictionary<uint, Value32>, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, FieldMetaData, ColumnMetaData, Value32[], Dictionary<uint, Value32>, Dictionary<long, string>, int, object>>
        {
            [typeof(float)] = (data, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => GetFieldValueArray<float>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(int)] = (data, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => GetFieldValueArray<int>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(uint)] = (data, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => GetFieldValueArray<uint>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(ulong)] = (data, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => GetFieldValueArray<ulong>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(ushort)] = (data, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => GetFieldValueArray<ushort>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(byte)] = (data, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => GetFieldValueArray<byte>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(string)] = (data, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => { int strOfs = GetFieldValueArray<int>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex]; return stringTable[strOfs]; },
        };

        public T GetField<T>(int fieldIndex, int arrayIndex = -1)
        {
            object value = null;

            if (fieldIndex >= m_reader.Meta.Length)
            {
                value = refData?.Id ?? 0;
                return (T)value;
            }

            m_data.Position = columnMeta[fieldIndex].RecordOffset;
            m_data.Offset = Offset;

            if (arrayIndex >= 0)
            {
                if (arrayReaders.TryGetValue(typeof(T), out var reader))
                    value = reader(m_data, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], m_reader.StringTable, arrayIndex);
                else
                    throw new Exception("Unhandled array type: " + typeof(T).Name);
            }
            else
            {
                if (simpleReaders.TryGetValue(typeof(T), out var reader))
                    value = reader(Id, m_data, m_recordsOffset, fieldMeta[fieldIndex], columnMeta[fieldIndex], palletData[fieldIndex], commonData[fieldIndex], m_reader.StringTable);
                else
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
            }

            return (T)value;
        }

        private static T GetFieldValue<T>(uint Id, BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<uint, Value32> commonData) where T : struct
        {
            switch (columnMeta.CompressionType)
            {
                case CompressionType.None:
                    int bitSize = 32 - fieldMeta.Bits;
                    if (bitSize > 0)
                        return r.ReadValue64(bitSize).GetValue<T>();
                    else
                        return r.ReadValue64(columnMeta.Immediate.BitWidth).GetValue<T>();
                case CompressionType.Immediate:
                case CompressionType.SignedImmediate:
                    return r.ReadValue64(columnMeta.Immediate.BitWidth).GetValue<T>();
                case CompressionType.Common:
                    if (commonData.TryGetValue(Id, out Value32 val))
                        return val.GetValue<T>();
                    else
                        return columnMeta.Common.DefaultValue.GetValue<T>();
                case CompressionType.Pallet:
                    uint palletIndex = r.ReadUInt32(columnMeta.Pallet.BitWidth);

                    T val1 = palletData[palletIndex].GetValue<T>();

                    return val1;
            }
            throw new Exception(string.Format("Unexpected compression type {0}", columnMeta.CompressionType));
        }

        private static T[] GetFieldValueArray<T>(BitReader r, FieldMetaData fieldMeta, ColumnMetaData columnMeta, Value32[] palletData, Dictionary<uint, Value32> commonData, int arraySize) where T : struct
        {
            switch (columnMeta.CompressionType)
            {
                case CompressionType.None:
                    int bitSize = 32 - fieldMeta.Bits;

                    T[] arr1 = new T[arraySize];

                    for (int i = 0; i < arr1.Length; i++)
                    {
                        if (bitSize > 0)
                            arr1[i] = r.ReadValue64(bitSize).GetValue<T>();
                        else
                            arr1[i] = r.ReadValue64(columnMeta.Immediate.BitWidth).GetValue<T>();
                    }

                    return arr1;
                case CompressionType.Immediate:
                case CompressionType.SignedImmediate:
                    T[] arr2 = new T[arraySize];

                    for (int i = 0; i < arr2.Length; i++)
                        arr2[i] = r.ReadValue64(columnMeta.Immediate.BitWidth).GetValue<T>();

                    return arr2;
                case CompressionType.PalletArray:
                    int cardinality = columnMeta.Pallet.Cardinality;

                    if (arraySize != cardinality)
                        throw new Exception("Struct missmatch for pallet array field?");

                    uint palletArrayIndex = r.ReadUInt32(columnMeta.Pallet.BitWidth);

                    T[] arr3 = new T[cardinality];

                    for (int i = 0; i < arr3.Length; i++)
                        arr3[i] = palletData[i + cardinality * (int)palletArrayIndex].GetValue<T>();

                    return arr3;
            }
            throw new Exception(string.Format("Unexpected compression type {0}", columnMeta.CompressionType));
        }

        internal WDC2Row Clone()
        {
            return (WDC2Row)MemberwiseClone();
        }
    }

    public class WDC2Reader : IEnumerable<KeyValuePair<uint, WDC2Row>>
    {
        private const int HeaderSize = 84 + 6 * 4;
        private const uint WDC2FmtSig = 0x32434457; // WDC2

        public int RecordsCount { get; private set; }
        public int FieldsCount { get; private set; }
        public int RecordSize { get; private set; }
        public int StringTableSize { get; private set; }
        public int MinIndex { get; private set; }
        public int MaxIndex { get; private set; }
        public int IdFieldIndex { get; private set; }

        private readonly FieldMetaData[] m_meta;
        public FieldMetaData[] Meta => m_meta;

        private uint[] m_indexData;
        public uint[] IndexData => m_indexData;

        private ColumnMetaData[] m_columnMeta;
        public ColumnMetaData[] ColumnMeta => m_columnMeta;

        private Value32[][] m_palletData;
        public Value32[][] PalletData => m_palletData;

        private Dictionary<uint, Value32>[] m_commonData;
        public Dictionary<uint, Value32>[] CommonData => m_commonData;

        public Dictionary<long, string> StringTable => m_stringsTable;

        private Dictionary<uint, WDC2Row> _Records = new Dictionary<uint, WDC2Row>();

        // normal records data
        private byte[] recordsData;
        private Dictionary<long, string> m_stringsTable;

        // sparse records data
        private byte[] sparseData;
        private SparseEntry[] sparseEntries;

        public WDC2Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDC2Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    throw new InvalidDataException(String.Format("WDC2 file is corrupted!"));
                }

                uint magic = reader.ReadUInt32();

                if (magic != WDC2FmtSig)
                {
                    throw new InvalidDataException(String.Format("WDC2 file is corrupted!"));
                }

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();
                uint tableHash = reader.ReadUInt32();
                uint layoutHash = reader.ReadUInt32();
                MinIndex = reader.ReadInt32();
                MaxIndex = reader.ReadInt32();
                int locale = reader.ReadInt32();
                int flags = reader.ReadUInt16();
                IdFieldIndex = reader.ReadUInt16();
                int totalFieldsCount = reader.ReadInt32();
                int packedDataOffset = reader.ReadInt32(); // Offset within the field where packed data starts
                int lookupColumnCount = reader.ReadInt32(); // count of lookup columns
                int columnMetaDataSize = reader.ReadInt32(); // 24 * NumFields bytes, describes column bit packing, {ushort recordOffset, ushort size, uint additionalDataSize, uint compressionType, uint packedDataOffset or commonvalue, uint cellSize, uint cardinality}[NumFields], sizeof(DBC2CommonValue) == 8
                int commonDataSize = reader.ReadInt32();
                int palletDataSize = reader.ReadInt32(); // in bytes, sizeof(DBC2PalletValue) == 4
                int unk0 = reader.ReadInt32();
                int unk1 = reader.ReadInt32();
                int unk2 = reader.ReadInt32();
                int someBlockSize = reader.ReadInt32();
                int NumRecords2 = reader.ReadInt32();
                int StringTableSize2 = reader.ReadInt32();
                int copyTableSize = reader.ReadInt32();
                int sparseTableOffset = reader.ReadInt32(); // absolute value, {uint offset, ushort size}[MaxId - MinId + 1]
                int indexDataSize = reader.ReadInt32(); // int indexData[IndexDataSize / 4]
                int referenceDataSize = reader.ReadInt32(); // uint NumRecords, uint minId, uint maxId, {uint id, uint index}[NumRecords], questionable usefulness...

                // field meta data
                m_meta = reader.ReadArray<FieldMetaData>(FieldsCount);

                // column meta data
                m_columnMeta = reader.ReadArray<ColumnMetaData>(FieldsCount);

                // pallet data
                m_palletData = new Value32[m_columnMeta.Length][];

                for (int i = 0; i < m_columnMeta.Length; i++)
                {
                    if (m_columnMeta[i].CompressionType == CompressionType.Pallet || m_columnMeta[i].CompressionType == CompressionType.PalletArray)
                    {
                        m_palletData[i] = reader.ReadArray<Value32>((int)m_columnMeta[i].AdditionalDataSize / 4);
                    }
                }

                // common data
                m_commonData = new Dictionary<uint, Value32>[m_columnMeta.Length];

                for (int i = 0; i < m_columnMeta.Length; i++)
                {
                    if (m_columnMeta[i].CompressionType == CompressionType.Common)
                    {
                        Dictionary<uint, Value32> commonValues = new Dictionary<uint, Value32>();
                        m_commonData[i] = commonValues;

                        for (int j = 0; j < m_columnMeta[i].AdditionalDataSize / 8; j++)
                            commonValues[reader.ReadUInt32()] = reader.Read<Value32>();
                    }
                }

                long recordsOffset = reader.BaseStream.Position;

                if ((flags & 0x1) == 0)
                {
                    // records data
                    recordsData = reader.ReadBytes(RecordsCount * RecordSize);

                    Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading

                    // string data
                    m_stringsTable = new Dictionary<long, string>();

                    for (int i = 0; i < StringTableSize;)
                    {
                        long oldPos = reader.BaseStream.Position;

                        m_stringsTable[oldPos] = reader.ReadCString();

                        i += (int)(reader.BaseStream.Position - oldPos);
                    }
                }
                else
                {
                    // sparse data with inlined strings
                    sparseData = reader.ReadBytes(sparseTableOffset - HeaderSize - Marshal.SizeOf<FieldMetaData>() * FieldsCount);

                    if (reader.BaseStream.Position != sparseTableOffset)
                        throw new Exception("r.BaseStream.Position != sparseTableOffset");

                    sparseEntries = reader.ReadArray<SparseEntry>(MaxIndex - MinIndex + 1);

                    if (sparseTableOffset != 0)
                        throw new Exception("Sparse Table NYI!");
                    else
                        throw new Exception("Sparse Table with zero offset?");
                }

                // index data
                m_indexData = reader.ReadArray<uint>(indexDataSize / 4);

                // duplicate rows data
                Dictionary<uint, uint> copyData = new Dictionary<uint, uint>();

                for (int i = 0; i < copyTableSize / 8; i++)
                    copyData[reader.ReadUInt32()] = reader.ReadUInt32();

                // reference data
                ReferenceData refData = null;

                if (referenceDataSize > 0)
                {
                    refData = new ReferenceData
                    {
                        NumRecords = reader.ReadUInt32(),
                        MinId = reader.ReadUInt32(),
                        MaxId = reader.ReadUInt32()
                    };

                    refData.Entries = reader.ReadArray<ReferenceEntry>((int)refData.NumRecords);
                }

                BitReader bitReader = new BitReader(recordsData);

                for (int i = 0; i < RecordsCount; ++i)
                {
                    bitReader.Position = 0;
                    bitReader.Offset = i * RecordSize;

                    WDC2Row rec = new WDC2Row(this, bitReader, recordsOffset, indexDataSize != 0 ? m_indexData[i] : 0xFFFFFFFF, refData?.Entries[i]);

                    if (indexDataSize != 0)
                        _Records.Add(m_indexData[i], rec);
                    else
                        _Records.Add(rec.Id, rec);

                    if (i % 1000 == 0)
                        Console.Write("\r{0} records read", i);
                }

                foreach (var copyRow in copyData)
                {
                    WDC2Row rec = _Records[copyRow.Value].Clone();
                    rec.Id = copyRow.Key;
                    _Records.Add(copyRow.Key, rec);
                }
            }
        }

        public bool HasRow(uint id)
        {
            return _Records.ContainsKey(id);
        }

        public WDC2Row GetRow(uint id)
        {
            if (!_Records.ContainsKey(id))
                return null;

            return _Records[id];
        }

        public IEnumerator<KeyValuePair<uint, WDC2Row>> GetEnumerator()
        {
            return _Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Records.GetEnumerator();
        }
    }
}
