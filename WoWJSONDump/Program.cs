using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib;
using WoWFormatLib.FileReaders;

namespace WoWJSONDump
{
    class Program
    {
        static void Main(string[] args)
        {
            M2Reader reader = new M2Reader("Z:\\WoD\\18522_full\\");
            reader.LoadM2(@"Creature\Alakir\AlAkir.m2");
            File.WriteAllText(@"Z:\WoD\model.json", JsonConvert.SerializeObject(reader.model, Formatting.Indented));
        }
    }
}
