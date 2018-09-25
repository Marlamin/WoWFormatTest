using System;
using System.IO;
using WoWFormatLib.Utils;

namespace ExporterCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Not enough arguments, needs program, filename, outdir and type (if filedataid)");
                return;
            }

            CASC.InitCasc(null, null, args[0]);

            if (int.TryParse(args[1], out var fileDataID))
            {
                if (args.Length == 4)
                {
                    switch (args[3])
                    {
                        case "wmo":
                            Exporters.glTF.WMOExporter.ExportWMO("wmo_" + fileDataID + ".wmo", null, args[2], fileDataID);
                            break;
                        case "m2":
                            Exporters.glTF.M2Exporter.ExportM2("m2_" + fileDataID + ".m2", null, null, args[2], fileDataID);
                            break;
                        case "adt":
                            Exporters.glTF.ADTExporter.ExportADT("adt_" + fileDataID + ".adt", args[2]);
                            break;
                        default:
                            Console.WriteLine("Unknown type: " + args[3]);
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("When specifiying a filedataid, please also set the 4 argument to the type of the file (wmo, m2, adt)");
                }
            }
            else
            {
                var target = args[1].Replace('/', '\\');

                var ext = Path.GetExtension(args[1]).ToLower();
                switch (ext)
                {
                    case ".m2":
                        Exporters.glTF.M2Exporter.ExportM2(target, null, null, args[2]);
                        break;
                    case ".adt":
                        Exporters.glTF.ADTExporter.ExportADT(target, args[2]);
                        break;
                    case ".wmo":
                        Exporters.glTF.WMOExporter.ExportWMO(target, null, args[2]);
                        break;
                    default:
                        Console.WriteLine("Unknown ext: " + ext);
                        break;
                }
            }
        }
    }
}
