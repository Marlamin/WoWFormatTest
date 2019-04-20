using System;
using System.Collections;
using System.Collections.Generic;
using DBCDumpHost.Utils;
using Microsoft.AspNetCore.Mvc;

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
            public string error;
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
            var searching = false;

            if (string.IsNullOrWhiteSpace(Request.Query["search[value]"]))
            {
                Logger.WriteLine("Serving data " + start + "," + length + " for dbc " + name + " (" + build + ") for draw " + draw);
            }
            else
            {
                searching = true;
                Logger.WriteLine("Serving data " + start + "," + length + " for dbc " + name + " (" + build + ") for draw " + draw + " with filter " + Request.Query["search[value]"]);
            }

            var searchValue = Request.Query["search[value]"];

            var result = new DataTablesResult();

            result.draw = draw;

            try
            {
                var storage = DBCManager.LoadDBC(name, build);
                result.recordsTotal = storage.Values.Count;

                var fields = DefinitionManager.definitionCache[(name, build)].GetFields();

                result.data = new List<List<string>>();

                var resultCount = 0;
                foreach (var item in storage.Values)
                {
                    var rowList = new List<string>();
                    var matches = false;

                    for (var i = 0; i < fields.Length; ++i)
                    {
                        var field = fields[i];

                        if (field.FieldType.IsArray)
                        {
                            var a = (Array)field.GetValue(item);

                            for (var j = 0; j < a.Length; j++)
                            {
                                var isEndOfArray = a.Length - 1 == j;
                                var val = a.GetValue(j).ToString();
                                if (searching)
                                {
                                    if (val.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase))
                                        matches = true;
                                }

                                rowList.Add(val);
                            }
                        }
                        else
                        {
                            var val = field.GetValue(item).ToString();
                            if (searching)
                            {
                                if (val.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase))
                                    matches = true;
                            }

                            rowList.Add(val);
                        }
                    }

                    if (searching)
                    {
                        if (matches)
                        {
                            resultCount++;
                            result.data.Add(rowList);
                        }
                    }
                    else
                    {
                        resultCount++;
                        result.data.Add(rowList);
                    }
                }

                result.recordsFiltered = resultCount;

                var takeLength = length;
                if ((start + length) > resultCount)
                {
                    takeLength = resultCount - start;
                }

                result.data = result.data.GetRange(start, takeLength);
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error occured during serving data: " + e.Message);
                result.error = e.Message.Replace(SettingManager.dbcDir, "");
            }

            return result;
        }
    }
}
