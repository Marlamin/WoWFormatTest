using CascStorageLib;
using CascStorageLib.Attributes;
using DBDefsLib;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
namespace DBCDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Not enough arguments: inputdb2 outputcsv");
                return;
            }

            var filename = args[0];
            var outputcsv = args[1];

            if (!File.Exists(filename))
            {
                throw new Exception("Input DB2 file does not exist!");
            }

            if (!Directory.Exists(Path.GetDirectoryName(outputcsv)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputcsv));
            }

            var reader = new WDC2Reader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));

            var defs = new Structs.DBDefinition();

            foreach (var file in Directory.GetFiles("definitions/"))
            {
                if (Path.GetFileNameWithoutExtension(file).ToLower() == Path.GetFileNameWithoutExtension(filename.ToLower()))
                {
                    defs = new DBDReader().Read(file);
                }
            }

            var writer = new StreamWriter(outputcsv);

            foreach (var definition in defs.versionDefinitions)
            {
                if (definition.layoutHashes.Contains(reader.LayoutHash.ToString("X8")))
                {
                    var aName = new AssemblyName("DynamicAssemblyExample");
                    var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
                    var mb = ab.DefineDynamicModule(aName.Name);
                    var tb = mb.DefineType(Path.GetFileNameWithoutExtension(filename) + "Struct", TypeAttributes.Public);

                    foreach (var field in definition.definitions)
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
                    var genericType = typeof(Storage<>).MakeGenericType(type);
                    var storage = (IDictionary)Activator.CreateInstance(genericType, filename);

                    if (storage.Values.Count == 0)
                    {
                        throw new Exception("No rows found!");
                    }

                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance);

                    var headerWritten = false;

                    foreach (var item in storage.Values)
                    {
                        // Write CSV header
                        if (!headerWritten)
                        {
                            for (var j = 0; j < fields.Length; ++j)
                            {
                                var field = fields[j];

                                var isEndOfRecord = fields.Length - 1 == j;

                                if (field.FieldType.IsArray)
                                {
                                    var a = (Array)field.GetValue(item);
                                    for (var i = 0; i < a.Length; i++)
                                    {
                                        var isEndOfArray = a.Length - 1 == i;

                                        writer.Write($"{field.Name}[{i}]");
                                        if (!isEndOfArray)
                                            writer.Write(",");
                                    }
                                }
                                else
                                {
                                    writer.Write(field.Name);
                                }

                                if (!isEndOfRecord)
                                    writer.Write(",");
                            }
                            headerWritten = true;
                            writer.WriteLine();
                        }

                        for (var i = 0; i < fields.Length; ++i)
                        {
                            var field = fields[i];

                            var isEndOfRecord = fields.Length - 1 == i;

                            if (field.FieldType.IsArray)
                            {
                                var a = (Array)field.GetValue(item);

                                for (var j = 0; j < a.Length; j++)
                                {
                                    var isEndOfArray = a.Length - 1 == j;
                                    writer.Write(a.GetValue(j));

                                    if (!isEndOfArray)
                                        writer.Write(",");
                                }
                            }
                            else
                            {
                                var value = field.GetValue(item);
                                if (value.GetType() == typeof(string))
                                    value = StringToCSVCell((string)value);

                                writer.Write(value);
                            }

                            if (!isEndOfRecord)
                                writer.Write(",");
                        }

                        writer.WriteLine();
                    }

                    writer.Dispose();
                    Environment.Exit(0);
                }
            }

            throw new Exception("Unable to find layouthash " + reader.LayoutHash.ToString("X8") + "!");
        }

        public static string StringToCSVCell(string str)
        {
            var mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                var sb = new StringBuilder();
                sb.Append("\"");
                foreach (var nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
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
