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
    [Route("api/data")]
    [ApiController]
    public class DataController : ControllerBase
    {
        public struct DataTablesResult
        {
            public int draw;
            public int recordsFiltered;
            public int recordsTotal;
            public List<List<string>> data;
        }

        // GET: data/
        [HttpGet]
        public string Get()
        {
            return "No DBC selected!";
        }

        // GET: data/name
        [HttpGet("{name}")]
        public DataTablesResult Get(string name, string build, int draw, int start, int length)
        {
            var result = new DataTablesResult();

            result.draw = draw;

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

            result.recordsTotal = storage.Values.Count;
            result.recordsFiltered = storage.Values.Count;

            var fields = rawType.GetFields();

            result.data = new List<List<string>>();

            var offset = 0;
            foreach (var item in storage.Values)
            {
                offset++;
                if (start > offset)
                    continue;

                if (offset > (start + length))
                    continue;

                var rowList = new List<string>();

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
                            rowList.Add(a.GetValue(j).ToString());
                        }
                    }
                    else
                    {
                        rowList.Add(field.GetValue(item).ToString());
                    }
                }

                result.data.Add(rowList);
            }

            return result;
        }
    }
}
