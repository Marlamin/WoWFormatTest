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

            DB2Reader<AreaPOIRecord> reader = new DB2Reader<AreaPOIRecord>("DBFilesClient\\AreaPOI.db2");
            for (int i = 0; i < reader.recordCount; i++)
            {
                Console.WriteLine(reader[i].ID + ": " + reader[i].name_lang);
            }
                //DBCReader<AreaTableRecord> reader = new DBCReader<AreaTableRecord>();
                //reader.LoadDBC("DBFilesClient\\AreaTable.dbc");
                /*Console.WriteLine(reader.header.record_count + " rows!");
                Console.WriteLine(reader.header.record_size + " row size!");
                Console.WriteLine(reader.header.field_count + " fields!");
                at.rows
                for (int i = 0; i < at.records.Count(); i++)
                {
                 //Console.WriteLine("Area: " + reader.stringblock[(int)reader.records[i].AreaName_lang] + " <" + reader.stringblock[(int)reader.records[i].ZoneName] +">");
                }*/
                Console.ReadLine();
        }
    }
}
