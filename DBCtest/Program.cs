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
            Console.WriteLine("CASC loaded");
            FileDataReader reader = new FileDataReader();
            reader.LoadDBC("DBFilesClient\\FileData.dbc");
            Console.WriteLine(reader.header.field_count);
            Console.WriteLine(reader.header.record_size);
            Console.WriteLine(reader.header.record_count + " records!");
            for (int i = 0; i < reader.records.Count(); i++)
            {
                if (reader.getString(reader.records[i].FileName) == "Serpent.M2")
                {
                    Console.WriteLine("Found ID in FileData.dbc: " + reader.records[i].ID);
                    CreatureModelDataReader cmdreader = new CreatureModelDataReader();
                    cmdreader.LoadDBC("DBFilesClient\\CreatureModelData.dbc");
                    for (int cmdi = 0; cmdi < cmdreader.records.Count(); cmdi++)
                    {
                        if (reader.records[i].ID == cmdreader.records[cmdi].fileDataID)
                        {
                            Console.WriteLine("Found Creature ID in CreatureModelData.dbc: " + cmdreader.records[cmdi].ID);
                            CreatureDisplayInfoReader cdireader = new CreatureDisplayInfoReader();
                            cdireader.LoadDBC("DBFilesClient\\CreatureDisplayInfo.dbc");
                            for (int cdii = 0; cdii < cdireader.records.Count(); cdii++)
                            {
                                if (cdireader.records[cdii].modelID == cmdreader.records[cmdi].ID)
                                {
                                    Console.WriteLine("Found a texture! Just fucking use " + getString(cdireader.records[cdii].textureVariation_0, cdireader.stringblock) + " already! Stopping here because fuck this.");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        public static string getString(uint offset, byte[] stringblock)
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(stringblock));
            File.WriteAllBytes("test.bin", stringblock);
            bin.BaseStream.Position = offset;
            return bin.ReadStringNull();
        }

    }
}
