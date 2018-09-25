using System;
using System.IO;
using WoWFormatLib.Utils;

namespace ExporterCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Not enough arguments, needs buildconfig, cdnconfig, filedataid, outdir and type (if filedataid)");
                return;
            }
            
            CASC.InitCasc("localhost:5005", args[0], args[1]);

            if (uint.TryParse(args[2], out var fileDataID))
            {
                switch (args[4])
                {
                    case "wmo":
                        Exporters.glTF.WMOExporter.ExportWMO("wmo_" + fileDataID + ".wmo", null, args[3], fileDataID);
                        break;
                    case "m2":
                        Exporters.glTF.M2Exporter.ExportM2("m2_" + fileDataID + ".m2", null, null, args[3], fileDataID);
                        break;
                    case "adt":
                        Exporters.glTF.ADTExporter.ExportADT("adt_" + fileDataID + ".adt", args[3]);
                        break;
                    default:
                        Console.WriteLine("Unknown type: " + args[3]);
                        break;
                }
            }
            else
            {
                throw new Exception("Do not support exporting by filename anymore!");
                //var target = args[1].Replace('/', '\\');

                //var ext = Path.GetExtension(args[1]).ToLower();
                //switch (ext)
                //{
                //    case ".m2":
                //        Exporters.glTF.M2Exporter.ExportM2(target, null, null, args[2]);
                //        break;
                //    case ".adt":
                //        Exporters.glTF.ADTExporter.ExportADT(target, args[2]);
                //        break;
                //    case ".wmo":
                //        Exporters.glTF.WMOExporter.ExportWMO(target, null, args[2]);
                //        break;
                //    default:
                //        Console.WriteLine("Unknown ext: " + ext);
                //        break;
                //}
            }
        }
    }
}
