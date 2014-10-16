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
    public class DB2Reader<T>
    {
        public DB2Header header;
        public T[] records;
        public Dictionary<int, string> stringblock;

        public DB2Reader()
        {
        }

        public void LoadDB2(string filename)
        {
            if(!CASC.IsCASCInit)
                CASC.InitCasc();

            if (!CASC.FileExists(filename))
            {
                new MissingFile(filename);
                return;
            }

            using (BinaryReader bin = new BinaryReader(File.Open(Path.Combine("data", filename), FileMode.Open)))
            {
                header = bin.Read<DB2Header>();

                records = new T[header.record_count];

                for (int i = 0; i < header.record_count; i++)
                {
                    records[i] = bin.Read<T>();
                }

                int stringblock_start = (int)bin.BaseStream.Position;

                stringblock = new Dictionary<int, string>();
                
                while (bin.BaseStream.Position != bin.BaseStream.Length)
                {
                    int index = (int)bin.BaseStream.Position - stringblock_start;
                    stringblock[index] = bin.ReadStringNull();
                }
            }
        }
    }
}
