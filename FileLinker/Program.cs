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
        static void insertEntry(MySqlCommand cmd, uint fileDataID, string desc)
        {
            if (fileDataID == 0)
                return;

            try
            {
                cmd.Parameters[1].Value = fileDataID;
                cmd.Parameters[2].Value = desc;
                cmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                if(!e.Message.StartsWith("Duplicate entry"))
                {
                    Console.WriteLine("Error inserting FDID (" + desc + "): " + e.Message);
                }
            }
        }

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

            CASCLib.Logger.Init();

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
                                insertEntry(insertCmd, textureID, "m2 texture");
                            }
                        }

                        if (reader.model.animFileDataIDs != null)
                        {
                            foreach (var animFileID in reader.model.animFileDataIDs)
                            {
                                insertEntry(insertCmd, animFileID.fileDataID, "m2 anim");
                            }
                        }

                        if (reader.model.skinFileDataIDs != null)
                        {
                            foreach (var skinFileID in reader.model.skinFileDataIDs)
                            {
                                insertEntry(insertCmd, skinFileID, "m2 skin");
                            }
                        }

                        if (reader.model.boneFileDataIDs != null)
                        {
                            foreach (var boneFileID in reader.model.boneFileDataIDs)
                            {
                                insertEntry(insertCmd, boneFileID, "m2 bone");
                            }
                        }

                        if (reader.model.recursiveParticleModelFileIDs != null)
                        {
                            foreach (var rpID in reader.model.recursiveParticleModelFileIDs)
                            {
                                insertEntry(insertCmd, rpID, "m2 recursive particle");
                            }
                        }

                        if (reader.model.geometryParticleModelFileIDs != null)
                        {
                            foreach (var gpID in reader.model.geometryParticleModelFileIDs)
                            {
                                insertEntry(insertCmd, gpID, "m2 geometry particle");
                            }
                        }

                        insertEntry(insertCmd, reader.model.skelFileID, "m2 skel");
                        insertEntry(insertCmd, reader.model.physFileID, "m2 phys");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            #endregion

            #region WMO
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
                                insertEntry(insertCmd, groupFileDataID, "wmo group");
                            }
                        }

                        if (wmo.doodadIds != null)
                        {
                            foreach (var doodadID in wmo.doodadIds)
                            {
                                if (inserted.Contains(doodadID))
                                    continue;

                                inserted.Add(doodadID);

                                insertEntry(insertCmd, doodadID, "wmo doodad");
                            }
                        }

                        if (wmo.textures == null && wmo.materials != null)
                        {
                            foreach (var material in wmo.materials)
                            {
                                if (material.texture1 == 0 || inserted.Contains(material.texture1))
                                    continue;

                                inserted.Add(material.texture1);
                                insertEntry(insertCmd, material.texture1, "wmo texture");

                                if (material.texture2 == 0 || inserted.Contains(material.texture2))
                                    continue;

                                inserted.Add(material.texture2);
                                insertEntry(insertCmd, material.texture2, "wmo texture");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
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
                    cmd.CommandText = "SELECT id, filename from wow_rootfiles WHERE type = 'wdt' AND id NOT IN (SELECT parent FROM wow_rootfiles_links) ORDER BY id DESC";
                }
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    //var filename = (string)reader["filename"];
                    var wdtid = uint.Parse(reader["id"].ToString());
                    //if (filename.Contains("_mpv") || filename.Contains("_lgt") || filename.Contains("_occ") || filename.Contains("_fogs"))
                    //   continue;
                    wdtids.Add(wdtid);
                }

                reader.Close();

                foreach (var wdtid in wdtids)
                {
                    Console.WriteLine("[WDT] Loading " + wdtid);

                    insertCmd.Parameters[0].Value = wdtid;
                    try
                    {
                        var wdtreader = new WDTReader();
                        wdtreader.LoadWDT(wdtid);

                        if (wdtreader.wdtfile.modf.id != 0)
                        {
                            Console.WriteLine("WDT has WMO ID: " + wdtreader.wdtfile.modf.id);
                            insertEntry(insertCmd, wdtreader.wdtfile.modf.id, "wdt wmo");
                        }

                        foreach (var records in wdtreader.stringTileFiles)
                        {
                            insertEntry(insertCmd, records.Value.rootADT, "root adt");
                            insertEntry(insertCmd, records.Value.tex0ADT, "tex0 adt");
                            insertEntry(insertCmd, records.Value.lodADT, "lod adt");
                            insertEntry(insertCmd, records.Value.obj0ADT, "obj0 adt");
                            insertEntry(insertCmd, records.Value.obj1ADT, "obj1 adt");
                            insertEntry(insertCmd, records.Value.mapTexture, "map texture");
                            insertEntry(insertCmd, records.Value.mapTextureN, "mapn texture");
                            insertEntry(insertCmd, records.Value.minimapTexture, "minimap texture");
                        }
                    }catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
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

                                inserted.Add(worldmodel.mwidEntry);
                                insertEntry(insertCmd, worldmodel.mwidEntry, "adt worldmodel");
                            }

                            foreach (var doodad in adtreader.adtfile.objects.models.entries)
                            {
                                if (inserted.Contains(doodad.mmidEntry))
                                    continue;

                                insertEntry(insertCmd, doodad.mmidEntry, "adt doodad");
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
