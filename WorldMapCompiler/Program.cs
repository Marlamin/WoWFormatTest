using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SereniaBLPLib;
using WoWFormatLib;
using WoWFormatLib.DBC;
using WoWFormatLib.Utils;

namespace WorldMapCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("settings.json", true, true).Build();

            var saveExplored = bool.Parse(config["saveExploredMaps"]);
            var saveUnexplored = bool.Parse(config["saveUnexploredMaps"]);
            var saveLayers = bool.Parse(config["saveMapLayers"]);

            if (saveExplored && !Directory.Exists("explored"))
            {
                Directory.CreateDirectory("explored");
            }

            if (saveUnexplored && !Directory.Exists("unexplored"))
            {
                Directory.CreateDirectory("unexplored");
            }

            if (saveLayers && !Directory.Exists("layers"))
            {
                Directory.CreateDirectory("layers");
            }

            var locale = CASCLib.LocaleFlags.enUS;

            if (config["locale"] != string.Empty)
            {
                switch (config["locale"])
                {
                    case "deDE":
                        locale = CASCLib.LocaleFlags.deDE;
                        break;
                    case "enUS":
                        locale = CASCLib.LocaleFlags.enUS;
                        break;
                    case "ruRU":
                        locale = CASCLib.LocaleFlags.ruRU;
                        break;
                    case "zhCN":
                        locale = CASCLib.LocaleFlags.zhCN;
                        break;
                    case "zhTW":
                        locale = CASCLib.LocaleFlags.zhTW;
                        break;
                }
            }

            if (config["installDir"] != string.Empty && Directory.Exists(config["installDir"]))
            {
                CASC.InitCasc(null, config["installDir"], config["program"], locale);
            }
            else
            {
                CASC.InitCasc(null, null, config["program"], locale);
            }

            using (var UIMapStream = CASC.cascHandler.OpenFile("DBFilesClient\\UIMap.db2"))
            using (var UIMapXArtStream = CASC.cascHandler.OpenFile("DBFilesClient\\UIMapXMapArt.db2"))
            using (var UIMapArtTileStream = CASC.cascHandler.OpenFile("DBFilesClient\\UIMapArtTile.db2"))
            using (var WorldMapOverlayStream = CASC.cascHandler.OpenFile("DBFilesClient\\WorldMapOverlay.db2"))
            using (var WorldMapOverlayTileStream = CASC.cascHandler.OpenFile("DBFilesClient\\WorldMapOverlayTile.db2"))
            {
                var UIMapReader = new WDC2Reader(UIMapStream);
                var UIMapXArtReader = new WDC2Reader(UIMapXArtStream);
                var UIMapArtTileReader = new WDC2Reader(UIMapArtTileStream);
                var WorldMapOverlayReader = new WDC2Reader(WorldMapOverlayStream);
                var WorldMapOverlayTileReader = new WDC2Reader(WorldMapOverlayTileStream);

                Console.WriteLine(); // new line after wdc2 debug output

                foreach (var mapRow in UIMapReader)
                {
                    var mapName = mapRow.Value.GetField<string>(0);

                    Console.WriteLine(mapRow.Value.Id + " = " + mapName);

                    foreach (var mxaRow in UIMapXArtReader)
                    {
                        var phaseID = mxaRow.Value.GetField<uint>(0); // PhaseID
                        var uiMapArtID = mxaRow.Value.GetField<uint>(1); // UIMapArtID
                        var uiMapID = mxaRow.Value.GetField<int>(2); // UIMapID

                        if (phaseID != 0)
                            continue; // Skip phase stuff for now

                        if (uiMapID == mapRow.Value.Id)
                        {
                            var maxRows = uint.MinValue;
                            var maxCols = uint.MinValue;
                            var tileDict = new Dictionary<string, int>();

                            foreach (var matRow in UIMapArtTileReader)
                            {
                                var matUiMapArtID = matRow.Value.GetField<int>(4);
                                if (matUiMapArtID == uiMapArtID)
                                {
                                    var fdid = matRow.Value.GetField<int>(3);
                                    var rowIndex = matRow.Value.GetField<byte>(0);
                                    var colIndex = matRow.Value.GetField<byte>(1);
                                    var layerIndex = matRow.Value.GetField<byte>(2);

                                    // Skip other layers for now
                                    if (layerIndex != 0)
                                    {
                                        continue;
                                    }

                                    if (rowIndex > maxRows)
                                    {
                                        maxRows = rowIndex;
                                    }

                                    if (colIndex > maxCols)
                                    {
                                        maxCols = colIndex;
                                    }

                                    tileDict.Add(rowIndex + "," + colIndex, fdid);
                                }
                            }

                            var res_x = (maxRows + 1) * 256;
                            var res_y = (maxCols + 1) * 256;

                            var bmp = new Bitmap((int)res_y, (int)res_x);

                            var g = Graphics.FromImage(bmp);

                            for (var cur_x = 0; cur_x < maxRows + 1; cur_x++)
                            {
                                for (var cur_y = 0; cur_y < maxCols + 1; cur_y++)
                                {
                                    var fdid = tileDict[cur_x + "," + cur_y];

                                    if (CASC.cascHandler.FileExists(fdid))
                                    {
                                        using (var stream = CASC.cascHandler.OpenFile(fdid))
                                        {
                                            try
                                            {
                                                var blp = new BlpFile(stream);
                                                g.DrawImage(blp.GetBitmap(0), cur_y * 256, cur_x * 256, new Rectangle(0, 0, 256, 256), GraphicsUnit.Pixel);
                                            }
                                            catch(Exception e)
                                            {
                                                Console.WriteLine("An error occured opening BLP with filedataid " + fdid);
                                            }
                                        }
                                    }
                                }
                            }

                            if (saveUnexplored)
                            {
                                bmp.Save("unexplored/ " + CleanFileName(mapRow.Value.Id + " - " + mapName + ".png"));
                            }

                            if (!saveLayers && !saveExplored)
                            {
                                continue;
                            }

                            foreach (var wmorow in WorldMapOverlayReader)
                            {
                                var WMOUIMapArtID = wmorow.Value.GetField<uint>(1);
                                var offsetX = wmorow.Value.GetField<short>(4);
                                var offsetY = wmorow.Value.GetField<short>(5);

                                uint maxWMORows = 0;
                                uint maxWMOCols = 0;
                                var wmoTileDict = new Dictionary<string, int>();

                                if (WMOUIMapArtID == uiMapArtID)
                                {
                                    foreach (var wmotrow in WorldMapOverlayTileReader)
                                    {
                                        var worldMapOverlayID = wmotrow.Value.GetField<int>(4);

                                        // something wrong in/around this check
                                        if (worldMapOverlayID == wmorow.Value.Id)
                                        {
                                            var fdid = wmotrow.Value.GetField<int>(3);
                                            var rowIndex = wmotrow.Value.GetField<byte>(0);
                                            var colIndex = wmotrow.Value.GetField<byte>(1);
                                            var layerIndex = wmotrow.Value.GetField<byte>(2);

                                            // Skip other layers for now
                                            if (layerIndex != 0)
                                            {
                                                continue;
                                            }

                                            if (rowIndex > maxWMORows)
                                            {
                                                maxWMORows = rowIndex;
                                            }

                                            if (colIndex > maxWMOCols)
                                            {
                                                maxWMOCols = colIndex;
                                            }

                                            wmoTileDict.Add(rowIndex + "," + colIndex, fdid);
                                        }
                                    }
                                }

                                if (wmoTileDict.Count == 0)
                                {
                                    continue;
                                }

                                var layerResX = (maxWMORows + 1) * 256;
                                var layerResY = (maxWMOCols + 1) * 256;

                                var layerBitmap = new Bitmap((int)layerResY, (int)layerResX);
                                var layerGraphics = Graphics.FromImage(layerBitmap);

                                for (var cur_x = 0; cur_x < maxWMORows + 1; cur_x++)
                                {
                                    for (var cur_y = 0; cur_y < maxWMOCols + 1; cur_y++)
                                    {
                                        var fdid = wmoTileDict[cur_x + "," + cur_y];

                                        if (CASC.cascHandler.FileExists(fdid))
                                        {
                                            using (var stream = CASC.cascHandler.OpenFile(fdid))
                                            {
                                                try
                                                {
                                                    var blp = new BlpFile(stream);
                                                    var posY = cur_y * 256 + offsetX;
                                                    var posX = cur_x * 256 + offsetY;

                                                    if (saveLayers)
                                                    {
                                                        layerGraphics.DrawImage(blp.GetBitmap(0), cur_y * 256, cur_x * 256, new Rectangle(0, 0, 256, 256), GraphicsUnit.Pixel);
                                                    }
                                                    g.DrawImage(blp.GetBitmap(0), posY, posX, new Rectangle(0, 0, 256, 256), GraphicsUnit.Pixel);
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine("An error occured opening BLP with filedataid " + fdid);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (saveLayers)
                                {
                                    if (!Directory.Exists("layers/" + CleanFileName(mapRow.Value.Id + " - " + mapName) + "/"))
                                    {
                                        Directory.CreateDirectory("layers/" + CleanFileName(mapRow.Value.Id + " - " + mapName) + "/");
                                    }
                                    layerBitmap.Save("layers/" + CleanFileName(mapRow.Value.Id + " - " + mapName) + "/" + wmorow.Value.Id + ".png");
                                }
                            }

                            if (saveExplored)
                            {
                                bmp.Save("explored/ " + CleanFileName(mapRow.Value.Id + " - " + mapName + ".png"));
                            }
                        }
                    }
                }
            }
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}
