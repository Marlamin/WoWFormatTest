using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CascStorageLib;
using WoWFormatLib.Utils;

namespace OBJExporterUI
{
    public class DBCManager
    {
        private static Dictionary<(string, string), IDictionary> _dbcCache = new Dictionary<(string, string), IDictionary>();

        public static IDictionary LoadDBC(string name, string build, bool fromCache = false)
        {
            if (name.Contains("."))
            {
                throw new Exception("Invalid DBC name!");
            }

            if (string.IsNullOrEmpty(build))
            {
                throw new Exception("No build given!");
            }

            if (fromCache)
            {
                if (_dbcCache.TryGetValue((name, build), out var cachedStore))
                    return cachedStore;
            }

            var filename = Path.Combine("cache", name + ".db2");

            using (var stream = CASC.OpenFile("DBFilesClient\\" + name + ".db2"))
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                if (!Directory.Exists(filename))
                {
                    Directory.CreateDirectory("cache");
                }
                File.WriteAllBytes(filename, ms.ToArray());
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("DBC not found on disk: " + filename);
            }

            var rawType = DefinitionManager.CompileDefinition(filename, build);
            var type = typeof(Storage<>).MakeGenericType(rawType);
            var instance = (IDictionary)Activator.CreateInstance(type, filename);

            if (fromCache)
            {
                _dbcCache[(name, build)] = instance;
            }

            return instance;
        }
    }
}
