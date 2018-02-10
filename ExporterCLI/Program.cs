using System;
using System.IO;
using WoWFormatLib.Utils;

namespace ExporterCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Not enough arguments, needs filename and outdir");
                return;
            }
            var ext = Path.GetExtension(args[0]).ToLower();
            switch (ext){
                case ".adt":
                    Exporters.glTF.ADTExporter.ExportADT(args[0], args[1]);
                    break;
                case ".wmo":
                    Exporters.glTF.WMOExporter.ExportWMO(args[0], null, args[1]);
                    break;
                default:
                    Console.WriteLine("Unsupported file: " + ext + ". Valid files: adt, wmo");
                    break;
            }
        }
    }
}
