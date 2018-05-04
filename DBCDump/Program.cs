using System;
using System.IO;
using WoWFormatLib.DBC;
namespace DBCDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Not enough arguments! Require source and target.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                throw new FileNotFoundException("File " + args[0] + " could not be found!");
            }

            DB2Reader reader;

            using (var stream = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bin = new BinaryReader(stream))
            {
                var identifier = new string(bin.ReadChars(4));
                stream.Position = 0;
                switch (identifier)
                {
                    case "WDC2":
                        reader = new WDC2Reader(stream);
                        break;
                    case "WDC1":
                        reader = new WDC1Reader(stream);
                        break;
                    default:
                        throw new Exception("DBC type " + identifier + " is not supported!");
                }
            }

            var dbd = new DBDefsLib.Structs.DBDefinition();
            foreach (var file in Directory.GetFiles("definitions/"))
            {
                if (Path.GetFileNameWithoutExtension(file).ToLower() == Path.GetFileNameWithoutExtension(args[0]).ToLower())
                {
                    var dbdef = new DBDefsLib.DBDReader();
                    dbd = dbdef.Read(file);
                }
            }

            var writer = new CsvHelper.CsvWriter(new StreamWriter(File.OpenWrite(args[1])));

            foreach (var versionDef in dbd.versionDefinitions)
            {
                // Check field sizes
                var fields = versionDef.definitions.Length;
                foreach (var definition in versionDef.definitions)
                {
                    if (definition.arrLength > 0)
                    {
                        fields += definition.arrLength;
                        fields -= 1;
                    }

                    if (definition.isNonInline)
                    {
                        fields -= 1;
                    }

                    if (definition.isRelation)
                    {
                        fields -= 1;
                    }
                }

                if(fields == reader.FieldsCount)
                {
                    // Field count matches, let's use this one I guess.... 
                    foreach(var row in reader)
                    {
                        var fieldPos = 0;
                        foreach(var definition in versionDef.definitions)
                        {
                            if(definition.isNonInline && definition.isID)
                            {
                                writer.WriteField(row.Key);
                            }
                            else
                            {
                                //var type = Type.GetType(dbd.columnDefinitions[definition.name].type);
                                //Console.WriteLine(row.Value.GetField<typeof(type)>(fieldPos));
                                switch (dbd.columnDefinitions[definition.name].type)
                                {
                                    case "uint":
                                        switch (definition.size)
                                        {
                                            case 8:
                                                writer.WriteField(row.Value.GetField<byte>(fieldPos));
                                                break;
                                            case 16:
                                                writer.WriteField(row.Value.GetField<ushort>(fieldPos));
                                                break;
                                            case 32:
                                                writer.WriteField(row.Value.GetField<uint>(fieldPos));
                                                break;
                                        }
                                        break;
                                    case "int":
                                        switch (definition.size)
                                        {
                                            case 8:
                                                writer.WriteField(row.Value.GetField<sbyte>(fieldPos));
                                                break;
                                            case 16:
                                                writer.WriteField(row.Value.GetField<short>(fieldPos));
                                                break;
                                            case 32:
                                                writer.WriteField(row.Value.GetField<int>(fieldPos));
                                                break;
                                        }
                                        break;
                                    case "locstring":
                                    case "string":
                                        writer.WriteField(row.Value.GetField<string>(fieldPos));
                                        break;
                                    case "float":
                                        writer.WriteField(row.Value.GetField<float>(fieldPos));
                                        break;
                                    default:
                                        throw new Exception("Unhandled type: " + dbd.columnDefinitions[definition.name].type);
                                }
                                fieldPos++;
                            }
                        }
                        writer.NextRecord();
                    }
                    writer.Flush();
                    writer.Dispose();
                    return;
                }
            }
        }
    }
}
