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
            public KeyValuePair<string, string>[] fks;
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
            var result = new HeaderResult();

            if (string.IsNullOrEmpty(build))
            {
                throw new Exception("No build given!");
            }

            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json", optional: false, reloadOnChange: true).Build();
            var dbcdir = config.GetSection("config")["dbcdir"];
            var filename = Path.Combine(dbcdir, build, "dbfilesclient", name + ".db2");

            if (name.Contains("."))
            {
                throw new Exception("Invalid DBC name!");
            }

            if (!System.IO.File.Exists(filename))
            {
                throw new FileNotFoundException("DBC not found on disk!");
            }

            var dbc = Path.GetFileNameWithoutExtension(name);
            var rawType = DefinitionManager.CompileDefinition(filename, build);
            var type = typeof(Storage<>).MakeGenericType(rawType);
            var storage = (IDictionary)Activator.CreateInstance(type, filename);

            if (storage.Values.Count == 0)
            {
                throw new Exception("No rows found!");
            }

            var definition = DefinitionManager.definitionLookup[name];

            var fields = rawType.GetFields();
            result.headers = new List<string>();
            var fks = new Dictionary<string, string>();
            foreach(var item in storage.Values)
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

                            result.headers.Add($"{field.Name}[{i}]");

                            foreach(var columnDef in definition.columnDefinitions)
                            {
                                if(columnDef.Key == field.Name && columnDef.Value.foreignTable != null)
                                {
                                    fks.Add($"{field.Name}[{i}]", columnDef.Value.foreignTable + "::" + columnDef.Value.foreignColumn);
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
                                fks.Add(field.Name, columnDef.Value.foreignTable + "::" + columnDef.Value.foreignColumn);
                            }
                        }
                    }
                }

                break;
            }

            result.fks = fks.ToArray();
            return result;
        }
    }
}
