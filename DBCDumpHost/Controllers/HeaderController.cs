using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CascStorageLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DBCDumpHost.Controllers
{
    [Route("api/header")]
    [ApiController]
    public class HeaderController : ControllerBase
    {
        public struct HeaderResult
        {
            public List<string> headers;
            public Dictionary<string, string> fks;
        }

        // GET: api/DBC
        [HttpGet]
        public string Get()
        {
            return "No DBC selected!";
        }

        // GET: api/DBC/name
        [HttpGet("{name}")]
        public HeaderResult Get(string name, string build)
        {
            Console.WriteLine("Handling headers for " + name + " (" + build + ")");

            var result = new HeaderResult();

            if (string.IsNullOrEmpty(build))
            {
                throw new Exception("No build given!");
            }

            var filename = Path.Combine(SettingManager.dbcDir, build, "dbfilesclient", name + ".db2");

            if (name.Contains("."))
            {
                throw new Exception("Invalid DBC name!");
            }

            var storage = DBCManager.LoadDBC(filename, build);

            if (storage.Values.Count == 0)
            {
                throw new Exception("No rows found!");
            }

            if (!DefinitionManager.definitionLookup.ContainsKey(name))
            {
                throw new KeyNotFoundException("Definition for " + name);
            }

            var definition = DefinitionManager.definitionLookup[name];

            var fields = DefinitionManager.definitionCache[(filename, build)].GetFields();

            result.headers = new List<string>();
            result.fks = new Dictionary<string, string>();
            foreach(var item in storage.Values)
            {
                for (var j = 0; j < fields.Length; ++j)
                {
                    var field = fields[j];

                    if (field.FieldType.IsArray)
                    {
                        var a = (Array)field.GetValue(item);
                        for (var i = 0; i < a.Length; i++)
                        {
                            var isEndOfArray = a.Length - 1 == i;

                            result.headers.Add($"{field.Name}[{i}]");

                            foreach(var columnDef in definition.columnDefinitions)
                            {
                                if(columnDef.Key == field.Name && columnDef.Value.foreignTable != null)
                                {
                                    result.fks.Add($"{field.Name}[{i}]", columnDef.Value.foreignTable + "::" + columnDef.Value.foreignColumn);
                                }
                            }
                        }
                    }

                    else
                    {
                        result.headers.Add(field.Name);

                        foreach (var columnDef in definition.columnDefinitions)
                        {
                            if (columnDef.Key == field.Name && columnDef.Value.foreignTable != null)
                            {
                                result.fks.Add(field.Name, columnDef.Value.foreignTable + "::" + columnDef.Value.foreignColumn);
                            }
                        }
                    }
                }

                break;
            }

            return result;
        }
    }
}
