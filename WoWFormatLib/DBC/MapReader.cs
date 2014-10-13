using System;
using System.Collections.Generic;
using System.IO;

namespace WoWFormatLib.DBC
{
    public class MapReader
    {
        private string basedir;
        public bool useCASC = false;

        public MapReader(string basedir)
        {
            this.basedir = basedir;
        }

        public Dictionary<int, string> GetMaps()
        {
            var maps = new Dictionary<int, string>();
            string fullpath;
            if (useCASC)
            {
                Utils.CASC.DownloadFile(@"DBFilesClient\Map.dbc");
                if (!File.Exists(Path.Combine("data", @"DBFilesClient\Map.dbc")))
                {
                    new WoWFormatLib.Utils.MissingFile(@"DBFilesClient\Map.dbc");
                    return maps;
                }
                else
                {
                    fullpath = Path.Combine("data", @"DBFilesClient\Map.dbc");
                }
            }
            else
            {
                if (!File.Exists(Path.Combine(basedir, @"DBFilesClient\Map.dbc")))
                {
                    new WoWFormatLib.Utils.MissingFile(@"DBFilesClient\Map.dbc");
                    return maps;
                }
                else
                {
                    fullpath = Path.Combine(basedir, @"DBFilesClient\Map.dbc");
                }
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