using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SereniaBLPLib;
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

            using (var UIMapStream = CASC.OpenFile("DBFilesClient\\UIMap.db2"))
            using (var UIMapXArtStream = CASC.OpenFile("DBFilesClient\\UIMapXMapArt.db2"))
            using (var UIMapArtTileStream = CASC.OpenFile("DBFilesClient\\UIMapArtTile.db2"))
            using (var WorldMapOverlayStream = CASC.OpenFile("DBFilesClient\\WorldMapOverlay.db2"))
            using (var WorldMapOverlayTileStream = CASC.OpenFile("DBFilesClient\\WorldMapOverlayTile.db2"))
            {
                if (!Directory.Exists("dbcs"))
                {
                    Directory.CreateDirectory("dbcs");
                }

                var uimapfs = File.Create("dbcs/UIMap.db2");
                UIMapStream.CopyTo(uimapfs);
                uimapfs.Close();

                var uimapxartfs = File.Create("dbcs/UIMapXMapArt.db2");
                UIMapXArtStream.CopyTo(uimapxartfs);
                uimapxartfs.Close();

                var uimapatfs = File.Create("dbcs/UIMapArtTile.db2");
                UIMapArtTileStream.CopyTo(uimapatfs);
                uimapatfs.Close();

                var wmofs = File.Create("dbcs/WorldMapOverlay.db2");
                WorldMapOverlayStream.CopyTo(wmofs);
                wmofs.Close();

                var wmotfs = File.Create("dbcs/WorldMapOverlayTile.db2");
                WorldMapOverlayTileStream.CopyTo(wmotfs);
                wmotfs.Close();
            }

            var UIMap = DBCManager.LoadDBC("UIMap", CASC.BuildName);
            var UIMapXArt = DBCManager.LoadDBC("UIMapXMapArt", CASC.BuildName);
            var UIMapArtTile = DBCManager.LoadDBC("UIMapArtTile", CASC.BuildName);
            var WorldMapOverlay = DBCManager.LoadDBC("WorldMapOverlay", CASC.BuildName);
            var WorldMapOverlayTile = DBCManager.LoadDBC("WorldMapOverlayTile", CASC.BuildName);

            Console.WriteLine(); // new line after wdc2 debug output

            foreach (dynamic mapRow in UIMap)
            {
                var mapName = mapRow.Value.Name_lang;

                Console.WriteLine(mapRow.Key + " = " + mapName);

                foreach (dynamic mxaRow in UIMapXArt)
                {
                    var uiMapArtID = mxaRow.Value.UiMapArtID;
                    var uiMapID = mxaRow.Value.UiMapID;

                    if (mxaRow.Value.PhaseID != 0)
                        continue; // Skip phase stuff for now

                    if (uiMapID == mapRow.Key)
                    {
                        var maxRows = uint.MinValue;
                        var maxCols = uint.MinValue;
                        var tileDict = new Dictionary<string, int>();

                        foreach (dynamic matRow in UIMapArtTile)
                        {
                            var matUiMapArtID = matRow.Value.UiMapArtID;
                            if (matUiMapArtID == uiMapArtID)
                            {
                                var fdid = matRow.Value.FileDataID;
                                var rowIndex = matRow.Value.RowIndex;
                                var colIndex = matRow.Value.ColIndex;
                                var layerIndex = matRow.Value.LayerIndex;

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

                                if (CASC.FileExists((uint)fdid))
                                {
                                    using (var stream = CASC.OpenFile((uint)fdid))
                                    {
                                        try
                                        {
                                            var blp = new BlpFile(stream);
                                            g.DrawImage(blp.GetBitmap(0), cur_y * 256, cur_x * 256, new Rectangle(0, 0, 256, 256), GraphicsUnit.Pixel);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("An error occured opening BLP with filedataid " + fdid);
                                        }
                                    }
                                }
                            }
                        }

                        if (saveUnexplored)
                        {
                            bmp.Save("unexplored/ " + CleanFileName(mapRow.Key + " - " + mapName + ".png"));
                        }

                        if (!saveLayers && !saveExplored)
                        {
                            continue;
                        }

                        foreach (dynamic wmorow in WorldMapOverlay)
                        {
                            var WMOUIMapArtID = wmorow.Value.UiMapArtID;
                            var offsetX = wmorow.Value.OffsetX;
                            var offsetY = wmorow.Value.OffsetY;

                            uint maxWMORows = 0;
                            uint maxWMOCols = 0;
                            var wmoTileDict = new Dictionary<string, int>();

                            if (WMOUIMapArtID == uiMapArtID)
                            {
                                foreach (dynamic wmotrow in WorldMapOverlayTile)
                                {
                                    var worldMapOverlayID = wmotrow.Value.WorldMapOverlayID;

                                    // something wrong in/around this check
                                    if (worldMapOverlayID == wmorow.Key)
                                    {
                                        var fdid = wmotrow.Value.FileDataID;
                                        var rowIndex = wmotrow.Value.RowIndex;
                                        var colIndex = wmotrow.Value.ColIndex;
                                        var layerIndex = wmotrow.Value.LayerIndex;

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

                                    if (CASC.FileExists((uint)fdid))
                                    {
                                        using (var stream = CASC.OpenFile((uint)fdid))
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
                                if (!Directory.Exists("layers/" + CleanFileName(mapRow.Key + " - " + mapName) + "/"))
                                {
                                    Directory.CreateDirectory("layers/" + CleanFileName(mapRow.Key + " - " + mapName) + "/");
                                }
                                layerBitmap.Save("layers/" + CleanFileName(mapRow.Key + " - " + mapName) + "/" + wmorow.Key + ".png");
                            }
                        }

                        if (saveExplored)
                        {
                            bmp.Save("explored/ " + CleanFileName(mapRow.Key + " - " + mapName + ".png"));
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
