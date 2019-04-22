using System;
using System.Collections.Generic;
using System.IO;
using WoWFormatLib.FileReaders;
using System.Security.Cryptography;
using WoWFormatLib;
using System.IO.Compression;
using System.Linq;

namespace WoWFormatTest
{
    internal class Program
    {
        private static Dictionary<string, List<byte>> preShaderMD5Total = new Dictionary<string, List<byte>>();
        private static Dictionary<string, List<byte>> postShaderMD5Total = new Dictionary<string, List<byte>>();

        private static void Main(string[] args)
        {
            var reader = new BLSReader();
            //reader.LoadBLS(File.OpenRead(@"D:\shaders\shaders_30093\unknown\\FILEDATA_1106926.bls"));
            //File.WriteAllBytes("out.bin", reader.targetStream.ToArray());
            foreach (var file in Directory.GetFiles(@"D:\shaders\shaders_30093\unknown", "*.bls", SearchOption.AllDirectories))
            {
                try
                {
                    reader.LoadBLS(File.OpenRead(file));
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error loading shader " + file + ": " + e.Message);
                    Console.ResetColor();
                    continue;
                }
                var cleanName = file;
                for (var i = 0; i < reader.shaderFile.decompressedBlocks.Count; i++)
                {
                    using (var md5 = MD5.Create())
                    {
                        var rawhash = md5.ComputeHash(reader.shaderFile.decompressedBlocks[i]);
                        if (!preShaderMD5Total.ContainsKey(cleanName))
                        {
                            preShaderMD5Total.Add(cleanName, new List<byte>());
                        }

                        preShaderMD5Total[cleanName].AddRange(rawhash);
                    }
                }
            }

            var finalMD5Dict = new Dictionary<string, string>();
            foreach(var shader in preShaderMD5Total)
            {
                using (var md5 = MD5.Create())
                {
                    var finalHash = md5.ComputeHash(shader.Value.ToArray()).ToHexString();
                    if (finalMD5Dict.ContainsKey(finalHash))
                    {
                        Console.WriteLine(shader.Key + " has the same internal shaders as " + finalMD5Dict[finalHash]);
                    }
                    else
                    {
                        finalMD5Dict.Add(finalHash, shader.Key);
                    }
                }
            }

            foreach (var file in Directory.GetFiles(@"D:\shaders\shaders_30096\unknown", "*.bls", SearchOption.AllDirectories))
            {
                try
                {
                    reader.LoadBLS(File.OpenRead(file));
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error loading shader " + file + ": " + e.Message);
                    Console.ResetColor();
                    continue;
                }

                var cleanName = file;
                for (var i = 0; i < reader.shaderFile.decompressedBlocks.Count; i++)
                {
                    using (var md5 = MD5.Create())
                    {
                        var rawhash = md5.ComputeHash(reader.shaderFile.decompressedBlocks[i]);
                        if (!postShaderMD5Total.ContainsKey(cleanName))
                        {
                            postShaderMD5Total.Add(cleanName, new List<byte>());
                        }

                        postShaderMD5Total[cleanName].AddRange(rawhash);
                    }
                }
            }

            if (File.Exists("matches.txt"))
            {
                File.Delete("matches.txt");
            }

            var matches = new List<string>();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            var preShaderCopy = finalMD5Dict.Values.ToList();
            var postShaderCopy = postShaderMD5Total.Keys.ToList();
            foreach (var shader in postShaderMD5Total)
            {
                using (var md5 = MD5.Create())
                {
                    var finalHash = md5.ComputeHash(shader.Value.ToArray()).ToHexString();
                    if (finalMD5Dict.ContainsKey(finalHash))
                    {
                        matches.Add(Path.GetFileNameWithoutExtension(shader.Key).Replace("FILEDATA_", "") + ";" + Path.GetFileNameWithoutExtension(finalMD5Dict[finalHash]).Replace("FILEDATA_", ""));
                        preShaderCopy.Remove(finalMD5Dict[finalHash]);
                        postShaderCopy.Remove(shader.Key);
                    }
                }
            }

            File.WriteAllLines("matches.txt", matches.ToArray());
            File.WriteAllLines("leftovers-pre.txt", preShaderCopy.ToArray());
            File.WriteAllLines("leftovers-post.txt", postShaderCopy.ToArray());
            
            /*
            var preShaderCount = new Dictionary<string, string>();
            foreach (var shader in File.ReadAllLines("leftovers-pre.txt"))
            {
                reader.LoadBLS(File.OpenRead(shader));
                preShaderCount.Add(shader, reader.shaderFile.nShaders + "-" + reader.shaderFile.nCompressedChunks + "-" + reader.shaderFile.decompressedBlocks[0].Length);
            }
            //{[D:\shaders\shaders_30093\unknown\FILEDATA_1139694.bls, 126-74-8473]
            //{[D:\shaders\shaders_30096\unknown\FILEDATA_2976993.bls, 126-74-6320]}

            //{[D:\shaders\shaders_30093\unknown\FILEDATA_1316520.bls, 1440-595-8441]}
            //{[D:\shaders\shaders_30096\unknown\FILEDATA_2977492.bls, 1440-596-8461]}

            //{[D:\shaders\shaders_30093\unknown\FILEDATA_1316520.bls, 1440-595-8441]}
            //{[D:\shaders\shaders_30096\unknown\FILEDATA_2977492.bls, 1440-596-8461]

            //{[D:\shaders\shaders_30096\unknown\FILEDATA_2976993.bls, 126-74-6320]
            var postShaderCount = new Dictionary<string, string>();
            foreach (var shader in File.ReadAllLines("leftovers-post.txt"))
            {
                reader.LoadBLS(File.OpenRead(shader));
                postShaderCount.Add(shader, reader.shaderFile.nShaders + "-" + reader.shaderFile.nCompressedChunks + "-" + reader.shaderFile.decompressedBlocks[0].Length);
            }
            */
            //{[D:\shaders\shaders_30093\unknown\FILEDATA_1139630.bls, 640-287-8857]
            //{[D:\shaders\shaders_30096\unknown\FILEDATA_2977414.bls, 640-287-1]
           
        }
    }
}
