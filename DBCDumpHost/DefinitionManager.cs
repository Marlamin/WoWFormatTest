using System;
using System.Collections.Generic;
using System.IO;
using DBDefsLib;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Reflection.Emit;
using DB2FileReaderLib.NET.Attributes;
using DB2FileReaderLib.NET;
using DBCDumpHost.Utils;

namespace DBCDumpHost
{
    public static class DefinitionManager
    {
        public static Dictionary<string, Structs.DBDefinition> definitionLookup;
        public static Dictionary<(string, string), Type> definitionCache;
        private static ModuleBuilder mb;

        static DefinitionManager()
        {
            definitionCache = new Dictionary<(string, string), Type>();

            var aName = new AssemblyName("DBDefinitions");
            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            mb = ab.DefineDynamicModule(aName.Name);

            LoadDefinitions();
        }

        public static void LoadDefinitions()
        {
            var definitionsDir = SettingManager.definitionDir;
            Logger.WriteLine("Reloading definitions from directory " + definitionsDir);
            var newDict = new Dictionary<string, Structs.DBDefinition>();

            var reader = new DBDReader();

            foreach(var file in Directory.GetFiles(definitionsDir))
            {
                newDict.Add(Path.GetFileNameWithoutExtension(file).ToLower(), reader.Read(file));
            }

            definitionLookup = newDict;

            // Reset cache
            definitionCache = new Dictionary<(string, string), Type>();

            Logger.WriteLine("Loaded " + definitionLookup.Count + " definitions!");
        }

        public static Type CompileDefinition(string filename, string build, bool force = false)
        {
            var cleanDBName = Path.GetFileNameWithoutExtension(filename).ToLower();

            if (!force && definitionCache.TryGetValue((cleanDBName, build), out var knownType))
                return knownType;

            if (!File.Exists(filename))
            {
                throw new Exception("Input DB2 file does not exist!");
            }

            if (!definitionLookup.ContainsKey(cleanDBName))
            {
                throw new KeyNotFoundException("Definition for " + cleanDBName);
            }

            DB2Reader reader;

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bin = new BinaryReader(stream))
            {
                var identifier = new string(bin.ReadChars(4));
                stream.Position = 0;
                switch (identifier)
                {
                    case "WDC3":
                        reader = new WDC3Reader(stream);
                        break;
                    case "WDC2":
                    case "1SLC":
                        reader = new WDC2Reader(stream);
                        break;
                    case "WDC1":
                        reader = new WDC1Reader(stream);
                        break;
                    case "WDB6":
                        reader = new WDB6Reader(stream);
                        break;
                    case "WDB5":
                        reader = new WDB5Reader(stream);
                        break;
                    case "WDB4":
                        reader = new WDB4Reader(stream);
                        break;
                    case "WDB3":
                        reader = new WDB3Reader(stream);
                        break;
                    case "WDB2":
                        reader = new WDB2Reader(stream);
                        break;
                    case "WDBC":
                        reader = new WDBCReader(stream);
                        break;
                    default:
                        throw new Exception("DBC type " + identifier + " is not supported!");
                }
            }

            var defs = definitionLookup[cleanDBName];

            Structs.VersionDefinitions? versionToUse;

            if (!DBDefsLib.Utils.GetVersionDefinitionByLayoutHash(defs, reader.LayoutHash.ToString("X8"), out versionToUse))
            {
                if (!string.IsNullOrWhiteSpace(build))
                {
                    if (!DBDefsLib.Utils.GetVersionDefinitionByBuild(defs, new Build(build), out versionToUse))
                    {
                        throw new Exception("No valid definition found for this layouthash or build!");
                    }
                }
                else
                {
                    throw new Exception("No valid definition found for this layouthash and was not able to search by build!");
                }
            }
            
            var tb = mb.DefineType(Path.GetFileNameWithoutExtension(filename) + Path.GetRandomFileName().Replace(".", "") + "Struct", TypeAttributes.Public);

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
            definitionCache[(cleanDBName, build)] = type;
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
