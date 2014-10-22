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
        public static string[] getTexturesByModelFilename(string modelfilename, int flag, int repltex = 0)
        {
            List<string> filenames = new List<string>();
            if (flag == 1)
            {
                if (modelfilename.StartsWith("character", StringComparison.CurrentCultureIgnoreCase))
                {

                    DBCReader<ChrRaceRecord> reader = new DBCReader<ChrRaceRecord>("DBFilesClient\\ChrRaces.dbc");

                    int race_id = 1; //Default to human male
                    int gender_id = 0; 

                    for (int i = 0; i < reader.recordCount; i++)
                    {
                        if (modelfilename.IndexOf(reader[i].clientFileString, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            race_id = reader[i].ID;
                            if (modelfilename.IndexOf("female", 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            {
                                gender_id = 1;
                            }
                            else
                            {
                                gender_id = 0;
                            }
                            break;
                        }
                    }

                    DBCReader<CharSectionRecord> secreader = new DBCReader<CharSectionRecord>("DBFilesClient\\CharSections.dbc");
                    for (int i = 0; i < secreader.recordCount; i++)
                    {
                        if (secreader[i].raceID == race_id && secreader[i].sexID == gender_id && secreader[i].baseSection == 5)
                        {
                            filenames.Add(secreader[i].TextureName_0);
                        }
                    }

                    Console.WriteLine("Detected model as race ID " + race_id + " and gender " + gender_id);
                    Console.WriteLine("[NYI] Type 1 character texture lookups aren't implemented yet");
                }
                else if (modelfilename.StartsWith("creature", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("[NYI] Type 1 creature texture lookups aren't implemented yet");
                }
                else if (modelfilename.StartsWith("item", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("[NYI] Type 1 item texture lookups aren't implemented yet");
                }
                else
                {
                    Console.WriteLine("[NYI] Type 1 texture lookups aren't implemented yet (model: " + modelfilename + ")");
                }
            }
            else if (flag == 11)
            {
                DBCReader<FileDataRecord> reader = new DBCReader<FileDataRecord>("DBFilesClient\\FileData.dbc");
                string[] modelnamearr = modelfilename.Split('\\');
                string modelonly = modelnamearr[modelnamearr.Count() - 1];
                modelonly = Path.ChangeExtension(modelonly, ".M2");
                for (int i = 0; i < reader.recordCount; i++)
                {
                    if (reader[i].FileName == modelonly)
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
