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
    [Route("api/peek")]
    [ApiController]
    public class PeekController : ControllerBase
    {
        public struct DataTablesResult
        {
            public int draw;
            public int recordsFiltered;
            public int recordsTotal;
            public List<List<string>> data;
        }

        // GET: peek/
        [HttpGet]
        public string Get()
        {
            return "No DBC selected!";
        }

        // GET: peek/name
        [HttpGet("{name}")]
        public IActionResult Get(string name, string build, string bc, string col, int val)
        {
            Console.WriteLine("Handling foreign key row for " + name + "::" + col + " (" + build + "/" + bc + ") value " + val);

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

            var fields = DefinitionManager.definitionCache[(filename, build)].GetFields();

            var result = "<h4>Viewing record " + val + " in file " + Path.GetFileNameWithoutExtension(name) + "</h4><table class=\"table\">";

            var offset = 0;
            var recordFound = false;
            var page = 1;
            foreach (var item in storage.Values)
            {
                if (recordFound)
                    continue;

                offset++;
                if(offset == 25)
                {
                    page++;
                    offset = 0;
                }

                for (var i = 0; i < fields.Length; ++i)
                {
                    var field = fields[i];

                    if (field.Name != col)
                        continue;

                    // Don't think FKs to arrays are possible, so only check regular value
                    if (field.GetValue(item).ToString() == val.ToString())
                    {
                        for (var j = 0; j < fields.Length; ++j)
                        {
                            var subfield = fields[j];

                            if (subfield.FieldType.IsArray)
                            {
                                var a = (Array)subfield.GetValue(item);

                                for (var k = 0; k < a.Length; k++)
                                {
                                    var isEndOfArray = a.Length - 1 == k;
                                    result += "<tr><td>" + subfield.Name + "[" + k + "]</td><td>" + a.GetValue(k).ToString() + "</td></tr>";
                                }
                            }
                            else
                            {
                                result += "<tr><td>" + subfield.Name + "</td><td>" + subfield.GetValue(item).ToString() + "</td></tr>";
                            }
                        }

                        recordFound = true;
                    }
                }
            }

            result += "</table>";

            result += "<a target=\"_BLANK\" href=\"/dbc.php?dbc=" + Path.GetFileNameWithoutExtension(name) + ".db2&bc=" + bc + "#page=" + page + "\" class=\"btn btn-primary\">Go to record</a>";

            return new ContentResult()
            {
                Content = result,
                ContentType = "text/html",
            };
        }
    }
}
