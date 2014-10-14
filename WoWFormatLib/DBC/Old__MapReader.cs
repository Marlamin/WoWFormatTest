using System;
using System.Collections.Generic;
using System.IO;
using WoWFormatLib.Utils;

namespace WoWFormatLib.DBC
{
    public class MapReader
    {
        public MapReader()
        {
        }

        public Dictionary<int, string> GetMaps()
        {
            string fullpath;
            var maps = new Dictionary<int, string>();
            var filename = Path.Combine("DBFilesClient", "Map.dbc");

            if (CASC.FileExists(filename))
            {
                fullpath = Path.Combine("data", filename);
            }
            else
            {
                new WoWFormatLib.Utils.MissingFile(filename);
                return maps;
            }

            var Map = new CSDBCReader.DBCFile(fullpath);

            Map.Read(false);
            var dbc = Map.GetDataTable();
            for (var i = 0; i < dbc.Rows.Length; i++)
            {
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