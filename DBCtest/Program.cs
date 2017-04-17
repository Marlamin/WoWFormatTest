
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib;
using WoWFormatLib.DBC;
using WoWFormatLib.Utils;

namespace DBCtest
{
     class DatWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 5000;
            return w;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CASC.InitCasc(null, "C:\\World of Warcraft", "wow");

            var creatureData = new DBFilesClient.NET.Storage<CreatureEntry>(CASC.cascHandler.OpenFile(@"DBFilesClient/Creature.db2"));
            var cdiData = new DBFilesClient.NET.Storage<CreatureDisplayInfoEntry>(CASC.cascHandler.OpenFile(@"DBFilesClient/CreatureDisplayInfo.db2"));

            using (DatWebClient client = new DatWebClient())
            {
                foreach (var entry in creatureData)
                {
                    if (File.Exists("Creature/" + entry.Key + ".png")) continue;
                    if (entry.Value.DisplayID[0] == 0) continue;
                    try
                    {
                        client.DownloadFile(new Uri("http://localhost:12345/wow/creature/" + entry.Key), "Creature/" + entry.Key + ".png");
                    }
                    catch
                    {

                    }
                }

                foreach (var entry in cdiData)
                {
                    if (File.Exists("CDI/" + entry.Key + ".png")) continue;
                    if (entry.Value.ModelID == 0) continue;
                    try
                    {
                        client.DownloadFile(new Uri("http://localhost:12345/wow/creatureDisplayInfo/" + entry.Key), "CDI/" + entry.Key + ".png");
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
