using System;
using System.IO;
using WoWFormatLib.DBC;
namespace DBCDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Not enough arguments! Require source and target.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                throw new FileNotFoundException("File " + args[0] + " could not be found!");
            }
            
            using (var stream = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bin = new BinaryReader(stream))
            {
                var identifier = new string(bin.ReadChars(4));
                stream.Position = 0;
                switch (identifier)
                {
                    case "WDC2":
                        DumpWDC2(stream);
                        break;
                    //case "WDC1":
                    //    DumpWDC1(stream);
                    //    break;
                    default:
                        throw new Exception("DBC type " + identifier + " is not supported!");
                }
            }
        }

        private static void DumpWDC2(Stream stream)
        {
            var reader = new WDC2Reader(stream);
            //var dbdef = new DBDefsLib.DBDReader("");
        }
    }
}
