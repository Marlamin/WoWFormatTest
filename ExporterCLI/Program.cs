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
                Console.WriteLine("Not enough arguments, needs filename, outdir and type (if filedataid)");
                return;
            }

            CASC.InitCasc(null, null, "wowz");

            var target = args[0].Replace('/', '\\');

            var ext = Path.GetExtension(args[0]).ToLower();
            switch (ext){
                case ".adt":
                    Exporters.glTF.ADTExporter.ExportADT(target, args[1]);
                    break;
                case ".wmo":
                    Exporters.glTF.WMOExporter.ExportWMO(target, null, args[1]);
                    break;
                default:
                    if(args.Length == 3)
                    {
                        Console.WriteLine("Has type, assuming filedataid");
                        if(args[2] == "wmo")
                        {
                            var isNumeric = int.TryParse(target, out var filedataid);
                            if (isNumeric)
                            {
                                Exporters.glTF.WMOExporter.ExportWMO("wmo_" + args[0] + ".wmo", null, args[1], filedataid);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unsupported file: " + ext + ". Valid files: adt, wmo");
                    }
                    break;
            }
        }
    }
}
