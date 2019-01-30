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

            CASC.InitCasc("bnet.marlam.in", args[0], args[1]);

            var insertCmd = new MySqlCommand("INSERT INTO wow_rootfiles_links VALUES (@parent, @child, @type)", dbConn);
            insertCmd.Parameters.AddWithValue("@parent", 0);
            insertCmd.Parameters.AddWithValue("@child", 0);
            insertCmd.Parameters.AddWithValue("@type", "");
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

            var wmoids = new List<uint>();

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
            }

            foreach (var wmoid in wmoids)
            {
                if (CASC.FileExists(wmoid))
                {
                    Console.WriteLine("[WMO] Loading " + wmoid);
                    try
                    {
                        var reader = new WMOReader();
                        var wmo = reader.LoadWMO(wmoid);

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
            }
            #endregion

            #region WDT

            #endregion

        }
    }
}
