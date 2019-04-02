using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DB2FileReaderLib.NET;

namespace DBCDumpHost
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

            // TODO: Given the state of your shit keep it to false so you don't keep fucking your RAM sideways
            // This is a thing to consider when your memory issues are solved, with a timeout that releases it
            // (meaning concurrency, slim mutexes are better for that type of stuff than ConcurrentDictionary)
            // My assumption is that you end up calling this everywhere and if you load your entire DBC
            // for every page browsed, thats a big oops in response time.
            // -- Warpten.
            if (fromCache)
            {
                 if (_dbcCache.TryGetValue((name, build), out var cachedStore))
                     return cachedStore;
            }

            var filename = Path.Combine(SettingManager.dbcDir, build, "dbfilesclient", name + ".db2");

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
