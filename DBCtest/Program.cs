using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib;
using WoWFormatLib.DBC;
using WoWFormatLib.Utils;

namespace DBCtest
{
    class Program
    {
        static void Main(string[] args)
        {
            CASC.InitCasc();

            DB2Reader<CreatureRecord> reader = new DB2Reader<CreatureRecord>();
            reader.LoadDB2("DBFilesClient\\Creature.db2");
            Console.WriteLine(reader.header.record_count + " rows!");
            Console.WriteLine(reader.header.record_size + " row size!");
            Console.WriteLine(reader.header.field_count + " fields!");
            
            for (int i = 0; i < reader.records.Count(); i++)
            {
                if (reader.records[i].ID == 11326)
                {
                    Console.WriteLine("Creature: " + reader.stringblock[(int)reader.records[i].name] + " <" + reader.stringblock[(int)reader.records[i].title] +">");
                }
            }
            Console.ReadLine();
        }
    }
}
