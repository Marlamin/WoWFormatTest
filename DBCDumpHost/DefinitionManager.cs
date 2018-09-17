using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using DBDefsLib;

namespace DBCDumpHost
{
    public class DefinitionManager
    {
        public static Dictionary<string, Structs.DBDefinition> definitionLookup;

        public DefinitionManager(string definitionsDir)
        {
            LoadDefinitions(definitionsDir);
        }

        public static void LoadDefinitions(string definitionsDir)
        {
            Console.WriteLine("Reloading definitions from directory " + definitionsDir);
            var newDict = new Dictionary<string, Structs.DBDefinition>();

            var reader = new DBDReader();

            foreach(var file in Directory.GetFiles(definitionsDir))
            {
                newDict.Add(Path.GetFileNameWithoutExtension(file), reader.Read(file));
            }

            definitionLookup = newDict;
            Console.WriteLine("Done reloading " + definitionLookup.Count);
        }
    }
}
