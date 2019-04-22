using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace FileLinker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new Exception("Require buildconfig and cdnconfig (and yes for fullrun)");
            }

            var fullrun = false;

            if (args.Length == 3 && args[2] == "1")
            {
                Console.WriteLine("Doing full run!");
                fullrun = true;
            }

            // TODO: Use configuration stuff instead, but I don't want to figure that out right now. :)
            if (!File.Exists("connectionstring.txt"))
            {
                throw new Exception("connectionstring.txt not found!");
            }

            var dbConn = new MySqlConnection(File.ReadAllText("connectionstring.txt"));
            dbConn.Open();

            CASC.InitCasc("wow.tools", args[0], args[1]);

            var insertCmd = new MySqlCommand("INSERT INTO wow_rootfiles_links VALUES (@parent, @child, @type)", dbConn);
            insertCmd.Parameters.AddWithValue("@parent", 0);
            insertCmd.Parameters.AddWithValue("@child", 0);
            insertCmd.Parameters.AddWithValue("@type", "");
            insertCmd.Prepare();

            var insertUVFNCmd = new MySqlCommand("INSERT INTO wow_communityfiles VALUES (@id, @filename)", dbConn);
            insertUVFNCmd.Parameters.AddWithValue("@id", 0);
            insertUVFNCmd.Parameters.AddWithValue("@filename", 0);
            insertCmd.Prepare();

            #region M2
            var m2ids = new List<uint>();

            using (var cmd = dbConn.CreateCommand())
            {
                if (fullrun)
                {
                    cmd.CommandText = "SELECT id from wow_rootfiles WHERE type = 'm2' ORDER BY id DESC";
                }
                else
                {
                    Console.WriteLine("[M2] Generating list of files to process..");
                    cmd.CommandText = "SELECT id from wow_rootfiles WHERE type = 'm2' AND id NOT IN (SELECT parent FROM wow_rootfiles_links) ORDER BY id DESC";
                }

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    m2ids.Add(uint.Parse(reader["id"].ToString()));
                }

                reader.Close();
            }

            foreach (var m2 in m2ids)
            {
                if (CASC.FileExists(m2))
                {
                    Console.WriteLine("[M2] Loading " + m2);
                    try
                    {
                        var reader = new M2Reader();
                        reader.LoadM2(m2, false);

                        insertCmd.Parameters[0].Value = m2;

                        if (reader.model.textureFileDataIDs != null)
                        {
                            foreach (var textureID in reader.model.textureFileDataIDs)
                            {
                                if (textureID == 0)
                                    continue;

                                insertCmd.Parameters[1].Value = textureID;
                                insertCmd.Parameters[2].Value = "m2 texture";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (reader.model.animFileDataIDs != null)
                        {
                            foreach (var animFileID in reader.model.animFileDataIDs)
                            {
                                if (animFileID.fileDataID == 0)
                                    continue;

                                insertCmd.Parameters[1].Value = animFileID.fileDataID;
                                insertCmd.Parameters[2].Value = "m2 anim";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (reader.model.skinFileDataIDs != null)
                        {
                            foreach (var skinFileID in reader.model.skinFileDataIDs)
                            {
                                if (skinFileID == 0)
                                    continue;

                                insertCmd.Parameters[1].Value = skinFileID;
                                insertCmd.Parameters[2].Value = "m2 skin";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (reader.model.boneFileDataIDs != null)
                        {
                            foreach (var boneFileID in reader.model.boneFileDataIDs)
                            {
                                if (boneFileID == 0)
                                    continue;

                                insertCmd.Parameters[1].Value = boneFileID;
                                insertCmd.Parameters[2].Value = "m2 bone";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (reader.model.recursiveParticleModelFileIDs != null)
                        {
                            foreach (var rpID in reader.model.recursiveParticleModelFileIDs)
                            {
                                if (rpID == 0)
                                    continue;

                                insertCmd.Parameters[1].Value = rpID;
                                insertCmd.Parameters[2].Value = "m2 recursive particle";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (reader.model.geometryParticleModelFileIDs != null)
                        {
                            foreach (var gpID in reader.model.geometryParticleModelFileIDs)
                            {
                                if (gpID == 0)
                                    continue;

                                insertCmd.Parameters[1].Value = gpID;
                                insertCmd.Parameters[2].Value = "m2 geometry particle";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (reader.model.skelFileID != 0)
                        {
                            insertCmd.Parameters[1].Value = reader.model.skelFileID;
                            insertCmd.Parameters[2].Value = "m2 skel";
                            insertCmd.ExecuteNonQuery();
                        }

                        if (reader.model.physFileID != 0)
                        {
                            insertCmd.Parameters[1].Value = reader.model.physFileID;
                            insertCmd.Parameters[2].Value = "m2 phys";
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            #endregion

            #region WMO
            /*
            var wmoids = new List<uint>();

            var groupFixCMD = new MySqlCommand("UPDATE wow_rootfiles SET type = '_xxxwmo' WHERE id = @id LIMIT 1", dbConn);
            groupFixCMD.Parameters.AddWithValue("@id", 0);
            groupFixCMD.Prepare();

            using (var cmd = dbConn.CreateCommand())
            {
                if (fullrun)
                {
                    cmd.CommandText = "SELECT id from wow_rootfiles WHERE type = 'wmo' ORDER BY id DESC";
                }
                else
                {
                    Console.WriteLine("[WMO] Generating list of files to process..");
                    cmd.CommandText = "SELECT id from wow_rootfiles WHERE type = 'wmo' AND id NOT IN (SELECT parent FROM wow_rootfiles_links) ORDER BY id DESC";
                }
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    wmoids.Add(uint.Parse(reader["id"].ToString()));
                }

                reader.Close();
            }

            foreach (var wmoid in wmoids)
            {
                if (CASC.FileExists(wmoid))
                {
                    Console.WriteLine("[WMO] Loading " + wmoid);
                    try
                    {
                        var reader = new WMOReader();
                        var wmo = new WoWFormatLib.Structs.WMO.WMO();
                        try
                        {
                            wmo = reader.LoadWMO(wmoid);
                        }
                        catch (NotSupportedException e)
                        {
                            Console.WriteLine("[WMO] " + wmoid + " is a group WMO, fixing type and skipping..");
                            groupFixCMD.Parameters[0].Value = wmoid;
                            groupFixCMD.ExecuteNonQuery();
                            continue;
                        }

                        insertCmd.Parameters[0].Value = wmoid;

                        var inserted = new List<uint>();

                        if (wmo.groupFileDataIDs != null)
                        {
                            foreach (var groupFileDataID in wmo.groupFileDataIDs)
                            {
                                if (groupFileDataID == 0)
                                    continue;

                                insertCmd.Parameters[1].Value = groupFileDataID;
                                insertCmd.Parameters[2].Value = "wmo group";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (wmo.doodadIds != null)
                        {
                            foreach (var doodadID in wmo.doodadIds)
                            {
                                if (doodadID == 0 || inserted.Contains(doodadID))
                                    continue;

                                inserted.Add(doodadID);
                                insertCmd.Parameters[1].Value = doodadID;
                                insertCmd.Parameters[2].Value = "wmo doodad";
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        if (wmo.textures == null && wmo.materials != null)
                        {
                            foreach (var material in wmo.materials)
                            {
                                if (material.texture1 == 0 || inserted.Contains(material.texture1))
                                    continue;

                                inserted.Add(material.texture1);
                                insertCmd.Parameters[1].Value = material.texture1;
                                insertCmd.Parameters[2].Value = "wmo texture";
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }*/
            #endregion

            #region WDT
            var wdtids = new List<uint>();
            var wdtfullnamemap = new Dictionary<string, uint>();
            using (var cmd = dbConn.CreateCommand())
            {
                Console.WriteLine("[WDT] Generating list of WDT files..");
                cmd.CommandText = "SELECT id, filename from wow_rootfiles WHERE type = 'wdt' AND filename IS NOT NULL ORDER BY id DESC";
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var filename = (string)reader["filename"];
                    var wdtid = uint.Parse(reader["id"].ToString());
                    if (filename.Contains("_mpv") || filename.Contains("_lgt") || filename.Contains("_occ") || filename.Contains("_fogs"))
                        continue;
                    wdtfullnamemap.Add(filename, wdtid);
                }
            }

            using (var cmd = dbConn.CreateCommand())
            {
                if (fullrun)
                {
                    cmd.CommandText = "SELECT id, filename from wow_rootfiles WHERE type = 'wdt' ORDER BY id DESC";
                }
                else
                {
                    Console.WriteLine("[WDT] Generating list of files to process..");
                    cmd.CommandText = "SELECT id, filename from wow_rootfiles WHERE type = 'wdt' AND filename IS NOT NULL AND id NOT IN (SELECT parent FROM wow_rootfiles_links) ORDER BY id DESC";
                }
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var filename = (string)reader["filename"];
                    var wdtid = uint.Parse(reader["id"].ToString());
                    if (filename.Contains("_mpv") || filename.Contains("_lgt") || filename.Contains("_occ") || filename.Contains("_fogs"))
                        continue;
                    wdtids.Add(wdtid);
                }

                reader.Close();

                foreach (var wdtid in wdtids)
                {
                    Console.WriteLine("[WDT] Loading " + wdtid);

                    insertCmd.Parameters[0].Value = wdtid;

                    var wdtreader = new WDTReader();
                    wdtreader.LoadWDT(wdtid);

                    if (wdtreader.wdtfile.modf.id != 0)
                    {
                        Console.WriteLine("WDT has WMO ID: " + wdtreader.wdtfile.modf.id);
                        try
                        {
                            insertCmd.Parameters[1].Value = wdtreader.wdtfile.modf.id;
                            insertCmd.Parameters[2].Value = "wdt wmo";
                            insertCmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("WDT WMO: " + e.Message);
                        }
                    }

                    foreach (var records in wdtreader.stringTileFiles)
                    {
                        if (records.Value.rootADT != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.rootADT;
                                insertCmd.Parameters[2].Value = "root adt";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Root: " + e.Message);
                            }
                        }

                        if (records.Value.tex0ADT != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.tex0ADT;
                                insertCmd.Parameters[2].Value = "tex0 adt";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("TEX0: " + e.Message);
                            }
                        }

                        if (records.Value.lodADT != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.lodADT;
                                insertCmd.Parameters[2].Value = "lod adt";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("LOD: " + e.Message);
                            }
                        }

                        if (records.Value.obj0ADT != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.obj0ADT;
                                insertCmd.Parameters[2].Value = "obj0 adt";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("OBJ0: " + e.Message);
                            }
                        }

                        if (records.Value.obj1ADT != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.obj1ADT;
                                insertCmd.Parameters[2].Value = "obj1 adt";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("OBJ1: " + e.Message);
                            }
                        }

                        if (records.Value.mapTexture != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.mapTexture;
                                insertCmd.Parameters[2].Value = "map texture";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("MapT: " + e.Message);
                            }
                        }

                        if (records.Value.mapTextureN != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.mapTextureN;
                                insertCmd.Parameters[2].Value = "mapn texture";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("MapTN: " + e.Message);
                            }
                        }

                        if (records.Value.minimapTexture != 0)
                        {
                            try
                            {
                                insertCmd.Parameters[1].Value = records.Value.minimapTexture;
                                insertCmd.Parameters[2].Value = "minimap texture";
                                insertCmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Minimap: " + e.Message);
                            }
                        }
                    }
                }
            }
            #endregion

            #region ADT
            var adtids = new Dictionary<uint, Dictionary<(byte, byte), uint>>();
            var wdtmapping = new Dictionary<string, uint>();

            using (var cmd = dbConn.CreateCommand())
            {
                if (fullrun)
                {
                    cmd.CommandText = " SELECT id, filename from wow_rootfiles WHERE filename LIKE '%adt' AND filename NOT LIKE '%_obj0.adt' AND filename NOT LIKE '%_obj1.adt' AND filename NOT LIKE '%_lod.adt' AND filename NOT LIKE '%tex0.adt' AND filename NOT LIKE '%tex1.adt' ORDER BY id DESC ";
                }
                else
                {
                    Console.WriteLine("[ADT] Generating list of files to process..");
                    cmd.CommandText = " SELECT id, filename from wow_rootfiles WHERE filename LIKE '%adt' AND filename NOT LIKE '%_obj0.adt' AND filename NOT LIKE '%_obj1.adt' AND filename NOT LIKE '%_lod.adt' AND filename NOT LIKE '%tex0.adt' AND filename NOT LIKE '%tex1.adt' AND id NOT IN (SELECT parent FROM wow_rootfiles_links) ORDER BY id DESC";
                }
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var filename = (string)reader["filename"];
                    var mapname = filename.Replace("world/maps/", "").Substring(0, filename.Replace("world/maps/", "").IndexOf("/"));
                    var exploded = Path.GetFileNameWithoutExtension(filename).Split('_');

                    for (var i = 0; i < exploded.Length; i++)
                    {
                        //Console.WriteLine(i + ": " + exploded[i]);
                    }

                    byte tileX = 0;
                    byte tileY = 0;

                    if (!byte.TryParse(exploded[exploded.Length - 2], out tileX) || !byte.TryParse(exploded[exploded.Length - 1], out tileY))
                    {
                        throw new FormatException("An error occured converting coordinates from " + filename + " to bytes");
                    }

                    if (!wdtmapping.ContainsKey(mapname))
                    {
                        var wdtname = "world/maps/" + mapname + "/" + mapname + ".wdt";
                        if (!wdtfullnamemap.ContainsKey(wdtname))
                        {
                            Console.WriteLine("Unable to get filedataid for " + mapname + ", skipping...");
                            wdtmapping.Remove(mapname);
                            continue;
                        }
                        wdtmapping.Add(mapname, wdtfullnamemap[wdtname]);
                        if (wdtmapping[mapname] == 0)
                        {
                            // TODO: Support WDTs removed in current build
                            Console.WriteLine("Unable to get filedataid for " + mapname + ", skipping...");
                            wdtmapping.Remove(mapname);
                            continue;
                            /*
                            var wdtconn = new MySqlConnection(File.ReadAllText("connectionstring.txt"));
                            wdtconn.Open();
                            using (var wdtcmd = wdtconn.CreateCommand())
                            {
                                wdtcmd.CommandText = "SELECT id from wow_rootfiles WHERE filename = '" + wdtname + "'";
                                var wdtread = wdtcmd.ExecuteReader();
                                while (wdtread.Read())
                                {
                                    wdtmapping[mapname] = uint.Parse(wdtread["id"].ToString());
                                }
                            }
                            wdtconn.Close();*/
                        }

                        adtids.Add(wdtmapping[mapname], new Dictionary<(byte, byte), uint>());
                    }

                    var id = uint.Parse(reader["id"].ToString());

                    if (id == 0)
                    {
                        Console.WriteLine("Root ADT " + tileX + ", " + tileY + " with ID 0 on WDT " + wdtmapping[mapname]);
                        continue;
                    }

                    if (wdtmapping.ContainsKey(mapname))
                    {
                        adtids[wdtmapping[mapname]].Add((tileX, tileY), id);
                    }
                }

                reader.Close();

                foreach (var wdtid in adtids)
                {
                    foreach (var adtid in wdtid.Value)
                    {
                        var inserted = new List<uint>();
                        Console.WriteLine("[ADT] Loading " + adtid.Key.Item1 + ", " + adtid.Key.Item2 + "(" + adtid.Value + ")");

                        insertCmd.Parameters[0].Value = adtid.Value;

                        var adtreader = new ADTReader();
                        try
                        {
                            adtreader.LoadADT(wdtid.Key, adtid.Key.Item1, adtid.Key.Item2);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            continue;
                        }

                        if (adtreader.adtfile.objects.m2Names.filenames != null)
                        {
                            Console.WriteLine(adtid + " is still using old filenames, skipping!");
                        }
                        else
                        {
                            foreach (var worldmodel in adtreader.adtfile.objects.worldModels.entries)
                            {
                                if (inserted.Contains(worldmodel.mwidEntry))
                                    continue;

                                insertCmd.Parameters[1].Value = worldmodel.mwidEntry;
                                insertCmd.Parameters[2].Value = "adt worldmodel";
                                insertCmd.ExecuteNonQuery();
                                inserted.Add(worldmodel.mwidEntry);
                            }

                            foreach (var doodad in adtreader.adtfile.objects.models.entries)
                            {
                                if (inserted.Contains(doodad.mmidEntry))
                                    continue;

                                insertCmd.Parameters[1].Value = doodad.mmidEntry;
                                insertCmd.Parameters[2].Value = "adt doodad";
                                insertCmd.ExecuteNonQuery();
                                inserted.Add(doodad.mmidEntry);
                            }
                        }
                    }
                }
            }
            #endregion


        }
    }
}
