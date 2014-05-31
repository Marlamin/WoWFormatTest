using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSDBCReader;
using System.Configuration;
using System.IO;

namespace WoWFormatTest
{
    class MapReader
    {
        public Dictionary<int, string> GetMaps()
        {
            Dictionary<int, string> maps = new Dictionary<int, string>();
            string basedir = ConfigurationManager.AppSettings["basedir"];

            DBCFile Map = new CSDBCReader.DBCFile(Path.Combine(basedir, "DBFilesClient\\Map.dbc"));
            Map.Read(false);
            DBCDataTable dbc = Map.GetDataTable();
            for (int i = 0; i < dbc.Rows.Length; i++){
                DBCDataRow row = dbc.Rows[i];
                int mapid = 0;
                string mapname = "";
                for (int x = 0; x < row.Cells.Length; x++)
                {
                    DBCDataField field = row.Cells[x];

                    if (x == 0)
                    {
                        mapid = BitConverter.ToInt32(field.Value, 0);
                    }
                    if (x == 1)
                    {
                        int stringoffset = BitConverter.ToInt32(field.Value, 0);
                        mapname = Map.Reader.ReadString(stringoffset);
                    }       
                }
                maps.Add(mapid, mapname);
            }
            return maps;
        }
    }
}
