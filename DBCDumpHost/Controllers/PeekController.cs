using System;
using System.Collections.Generic;
using DBCDumpHost.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DBCDumpHost.Controllers
{
    [Route("api/peek")]
    [ApiController]
    public class PeekController : ControllerBase
    {
        public struct PeekResult
        {
            public List<(string, string)> values;
            public int offset;
        }

        // GET: peek/
        [HttpGet]
        public string Get()
        {
            return "No DBC selected!";
        }

        // GET: peek/name
        [HttpGet("{name}")]
        public PeekResult Get(string name, string build, string bc, string col, int val)
        {
            Logger.WriteLine("Serving foreign key row for " + name + "::" + col + " (" + build + "/" + bc + ") value " + val);

            var storage = DBCManager.LoadDBC(name, build);

            if (storage.Values.Count == 0)
            {
                throw new Exception("No rows found!");
            }

            var fields = DefinitionManager.definitionCache[(name, build)].GetFields();

            var result = new PeekResult();
            result.values = new List<(string, string)>();

            var offset = 0;
            var recordFound = false;
            foreach (var item in storage.Values)
            {
                if (recordFound)
                    continue;

                offset++;

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
                                    result.values.Add((subfield.Name + "[" + k + "]", a.GetValue(k).ToString()));
                                }
                            }
                            else
                            {
                                result.values.Add((subfield.Name, subfield.GetValue(item).ToString()));
                            }
                        }

                        recordFound = true;
                    }
                }
            }

            result.offset = offset;

            return result;
        }
    }
}
