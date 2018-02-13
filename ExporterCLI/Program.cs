using System;
using System.IO;
using WoWFormatLib.Utils;

namespace ExporterCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 3)
            {
                Console.WriteLine("Not enough arguments, needs program, filename, outdir and type (if filedataid)");
                return;
            }

            CASC.InitCasc(null, null, args[0]);

            var target = args[1].Replace('/', '\\');

            var ext = Path.GetExtension(args[1]).ToLower();
            switch (ext){
                case ".adt":
                    Exporters.glTF.ADTExporter.ExportADT(target, args[2]);
                    break;
                case ".wmo":
                    Exporters.glTF.WMOExporter.ExportWMO(target, null, args[2]);
                    break;
                default:
                    if(args.Length == 4)
                    {
                        Console.WriteLine("Has type, assuming filedataid");
                        if(args[3] == "wmo")
                        {
                            var isNumeric = int.TryParse(target, out var filedataid);
                            if (isNumeric)
                            {
                                Exporters.glTF.WMOExporter.ExportWMO("wmo_" + args[1] + ".wmo", null, args[2], filedataid);
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
