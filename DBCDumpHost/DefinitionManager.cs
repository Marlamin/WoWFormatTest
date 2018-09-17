using System;
using System.Collections.Generic;
using System.IO;
using DBDefsLib;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Reflection.Emit;
using CascStorageLib.Attributes;
using CascStorageLib;

namespace DBCDumpHost
{
    public class DefinitionManager
    {
        public static Dictionary<string, Structs.DBDefinition> definitionLookup;

        public DefinitionManager()
        {
            LoadDefinitions();
        }

        public static void LoadDefinitions()
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json", optional: false, reloadOnChange: true).Build();
            var definitionsDir = config.GetSection("config")["definitionsdir"];

            Console.WriteLine("Reloading definitions from directory " + definitionsDir);
            var newDict = new Dictionary<string, Structs.DBDefinition>();

            var reader = new DBDReader();

            foreach(var file in Directory.GetFiles(definitionsDir))
            {
                newDict.Add(Path.GetFileNameWithoutExtension(file).ToLower(), reader.Read(file));
            }

            definitionLookup = newDict;
            Console.WriteLine("Done reloading " + definitionLookup.Count);
        }

        public static Type CompileDefinition(string filename, string build)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Input DB2 file does not exist!");
            }

            DB2Reader reader;

            var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
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

            var defs = new Structs.DBDefinition();

            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json", optional: false, reloadOnChange: true).Build();
            foreach (var file in Directory.GetFiles(config.GetSection("config")["definitionsdir"]))
            {
                if (Path.GetFileNameWithoutExtension(file).ToLower() == Path.GetFileNameWithoutExtension(filename.ToLower()))
                {
                    defs = new DBDReader().Read(file);
                    break;
                }
            }

            Structs.VersionDefinitions? versionToUse;

            if (!Utils.GetVersionDefinitionByLayoutHash(defs, reader.LayoutHash.ToString("X8"), out versionToUse))
            {
                if (!string.IsNullOrWhiteSpace(build))
                {
                    if (!Utils.GetVersionDefinitionByBuild(defs, new Build(build), out versionToUse))
                    {
                        throw new Exception("No valid definition found for this layouthash or build!");
                    }
                }
                else
                {
                    throw new Exception("No valid definition found for this layouthash and was not able to search by build!");
                }
            }

            var aName = new AssemblyName("DynamicAssemblyExample");
            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule(aName.Name);
            var tb = mb.DefineType(Path.GetFileNameWithoutExtension(filename) + "Struct", TypeAttributes.Public);

            foreach (var field in versionToUse.Value.definitions)
            {
                var fbNumber = tb.DefineField(field.name, DBDefTypeToType(defs.columnDefinitions[field.name].type, field.size, field.isSigned, field.arrLength), FieldAttributes.Public);
                if (field.isID)
                {
                    var constructorParameters = new Type[] { };
                    var constructorInfo = typeof(IndexAttribute).GetConstructor(constructorParameters);
                    var displayNameAttributeBuilder = new CustomAttributeBuilder(constructorInfo, new object[] { });
                    fbNumber.SetCustomAttribute(displayNameAttributeBuilder);
                }
            }

            var type = tb.CreateType();
            return type;
        }

        private static Type DBDefTypeToType(string type, int size, bool signed, int arrLength)
        {
            if (arrLength == 0)
            {
                switch (type)
                {
                    case "int":
                        switch (size)
                        {
                            case 8:
                                return signed ? typeof(sbyte) : typeof(byte);
                            case 16:
                                return signed ? typeof(short) : typeof(ushort);
                            case 32:
                                return signed ? typeof(int) : typeof(uint);
                            case 64:
                                return signed ? typeof(long) : typeof(ulong);
                        }
                        break;
                    case "string":
                    case "locstring":
                        return typeof(string);
                    case "float":
                        return typeof(float);
                    default:
                        throw new Exception("oh lord jesus have mercy i don't know about type " + type);
                }
            }
            else
            {
                switch (type)
                {
                    case "int":
                        switch (size)
                        {
                            case 8:
                                return signed ? typeof(sbyte[]) : typeof(byte[]);
                            case 16:
                                return signed ? typeof(short[]) : typeof(ushort[]);
                            case 32:
                                return signed ? typeof(int[]) : typeof(uint[]);
                            case 64:
                                return signed ? typeof(long[]) : typeof(ulong[]);
                        }
                        break;
                    case "string":
                    case "locstring":
                        return typeof(string[]);
                    case "float":
                        return typeof(float[]);
                    default:
                        throw new Exception("oh lord jesus have mercy i don't know about type " + type);
                }
            }

            return typeof(int);
        }
    }
}
