using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib;
using WoWFormatLib.DBC;

namespace DBCtest
{
    class Program
    {
        static void Main(string[] args)
        {
            DBCReader reader = new DBCReader();
            reader.LoadDBC("DBFilesClient\\Map.dbc");
            Console.WriteLine(reader.header.field_count);
            Console.WriteLine(reader.header.record_size);
            Console.WriteLine(reader.header.record_count + " records!");
            for (int i = 0; i < reader.records.Count(); i++)
            {
                Console.WriteLine(reader.records[i].ID);
                Console.WriteLine(reader.getString(reader.records[i].Directory));
            }
            
            Console.ReadLine();
        }
    }
}
