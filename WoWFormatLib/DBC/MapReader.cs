using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSDBCReader;
using System.Configuration;
using System.IO;

namespace WoWFormatLib.DBC
{
    public class MapReader
    {
        public Dictionary<int, string> GetMaps()
        {
            var maps = new Dictionary<int, string>();
            var basedir = ConfigurationManager.AppSettings["basedir"];

            var Map = new CSDBCReader.DBCFile(Path.Combine(basedir, "DBFilesClient\\Map.dbc"));
            Map.Read(false);
            var dbc = Map.GetDataTable();
            for (var i = 0; i < dbc.Rows.Length; i++){
                var row = dbc.Rows[i];
                var mapid = 0;
                var mapname = "";
                for (var x = 0; x < row.Cells.Length; x++)
                {
                    var field = row.Cells[x];

                    if (x == 0)
                    {
                        mapid = BitConverter.ToInt32(field.Value, 0);
                    }
                    if (x == 1)
                    {
                        var stringoffset = BitConverter.ToInt32(field.Value, 0);
                        mapname = Map.Reader.ReadString(stringoffset);
                    }       
                }
                maps.Add(mapid, mapname);
            }
            return maps;
        }
    }
}
