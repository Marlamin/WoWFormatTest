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
            Console.WriteLine(reader.header.record_count + " records!");
            Console.ReadLine();
        }
    }
}
