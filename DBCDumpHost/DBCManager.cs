using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CascStorageLib;

namespace DBCDumpHost
{
    public class DBCManager
    {
        // GetDictionary should return 
        public Dictionary<int, object> GetDictionary(string name, string build)
        {
            throw new NotImplementedException();

            var dbc = Path.GetFileNameWithoutExtension(name);
            var rawType = DefinitionManager.CompileDefinition(name, build);
            var type = typeof(Storage<>).MakeGenericType(rawType);
            var storage = (IDictionary)Activator.CreateInstance(type, name);

            if (storage.Values.Count == 0)
            {
                throw new Exception("No rows found!");
            }

            var fields = rawType.GetFields();

            foreach (var item in storage.Values)
            {
                var rowList = new List<string>();

                for (var i = 0; i < fields.Length; ++i)
                {
                    var field = fields[i];

                    if (field.FieldType.IsArray)
                    {
                        var a = (Array)field.GetValue(item);

                        for (var j = 0; j < a.Length; j++)
                        {
                            var isEndOfArray = a.Length - 1 == j;
                            rowList.Add(a.GetValue(j).ToString());
                        }
                    }
                    else
                    {
                        rowList.Add(field.GetValue(item).ToString());
                    }
                }
            }
        }
    }
}
