using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CascStorageLib;
using Microsoft.AspNetCore.Mvc;

namespace DBCDumpHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{dbc}")]
        public ActionResult<IEnumerable<string>> Get(string dbc)
        {
            Console.WriteLine(dbc);
            var build = "8.0.1.27602";
            var filename = Path.Combine("dbcs", build, "dbfilesclient", dbc + ".db2");
            Console.WriteLine(filename);
            var rawType = DBCManager.CompileDefinition(filename, build);
            var type = typeof(Storage<>).MakeGenericType(rawType);

            var storage = (IDictionary)Activator.CreateInstance(type, filename);

            if (storage.Values.Count == 0)
            {
                throw new Exception("No rows found!");
            }

            var fields = rawType.GetFields();

            var values = new List<string>();
            foreach (var item in storage.Values)
            {
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
                            values.Add(a.GetValue(j).ToString());
                        }
                    }
                    else
                    {
                        values.Add(field.GetValue(item).ToString());
                    }
                }
            }
            return values.ToArray();
        }
    }
}
