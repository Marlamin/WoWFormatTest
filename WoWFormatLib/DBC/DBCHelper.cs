using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.DBC
{
    public class DBCHelper
    {
        public static string[] getTexturesByModelFilename(string modelfilename, int flag)
        {
            List<string> filenames = new List<string>();
            if (flag == 1 || flag == 11)
            {
                DBCReader<FileDataRecord> reader = new DBCReader<FileDataRecord>("DBFilesClient\\FileData.dbc");

                for (int i = 0; i < reader.recordCount; i++)
                {
                    if (reader[i].FileName == modelfilename + ".M2")
                    {
                        Console.WriteLine("Found ID in FileData.dbc: " + reader[i].ID);
                        DBCReader<CreatureModelDataRecord> cmdreader = new DBCReader<CreatureModelDataRecord>("DBFilesClient\\CreatureModelData.dbc");
                        for (int cmdi = 0; cmdi < cmdreader.recordCount; cmdi++)
                        {
                            if (reader[i].ID == cmdreader[cmdi].fileDataID)
                            {
                                Console.WriteLine("Found Creature ID in CreatureModelData.dbc: " + cmdreader[cmdi].ID);
                                DBCReader<CreatureDisplayInfoRecord> cdireader = new DBCReader<CreatureDisplayInfoRecord>("DBFilesClient\\CreatureDisplayInfo.dbc");
                                for (int cdii = 0; cdii < cdireader.recordCount; cdii++)
                                {
                                    if (cdireader[cdii].modelID == cmdreader[cmdi].ID && cdireader[cdii].textureVariation_0 != null)
                                    {
                                        filenames.Add(cdireader[cdii].textureVariation_0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
           
            string[] ret = filenames.Distinct().ToArray();
            return ret;
        }
    }
}
