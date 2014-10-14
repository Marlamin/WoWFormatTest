using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.DBC;
using WoWFormatLib.Utils;
namespace WoWFormatLib.DBC
{
    public class CreatureModelDataReader
    {
        public DBCHeader header;
        public CreatureModelDataRecord[] records;
        public byte[] stringblock;

        public CreatureModelDataReader()
        {
        }

        public void LoadDBC(string filename)
        {
            if (!CASC.FileExists(filename))
            {
                new MissingFile(filename);
                return; //well shit what now
            }

            using (BinaryReader bin = new BinaryReader(File.Open(Path.Combine("data", filename), FileMode.Open)))
            {
                header = bin.Read<DBCHeader>();

                records = new CreatureModelDataRecord[header.record_count];

                for (int i = 0; i < header.record_count; i++)
                {
                    records[i] = bin.Read<CreatureModelDataRecord>();
                }
               
                stringblock = bin.ReadBytes((int)header.string_block_size);
            }
        }

        public string getString(uint offset){
            BinaryReader bin = new BinaryReader(new MemoryStream(stringblock));
            bin.BaseStream.Position = offset;
            return bin.ReadStringNull();
        }
    }
}
