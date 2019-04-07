using System;
using System.IO;
using System.Text;
using DBCDumpHost.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DBCDumpHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        [Route("")]
        [Route("csv")]
        [HttpGet]
        public ActionResult ExportCSV(string name, string build)
        {
            try
            {
                var storage = DBCManager.LoadDBC(name, build);
                if (storage.Values.Count == 0)
                {
                    throw new Exception("No rows found!");
                }

                var fields = DefinitionManager.definitionCache[(name, build)].GetFields();

                var headerWritten = false;

                using (var exportStream = new MemoryStream())
                using (var exportWriter = new StreamWriter(exportStream))
                {
                    foreach (var item in storage.Values)
                    {
                        // Write CSV header
                        if (!headerWritten)
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

                                        exportWriter.Write($"{field.Name}[{i}]");
                                        if (!isEndOfArray)
                                            exportWriter.Write(",");
                                    }
                                }
                                else
                                {
                                    exportWriter.Write(field.Name);
                                }

                                if (!isEndOfRecord)
                                    exportWriter.Write(",");
                            }
                            headerWritten = true;
                            exportWriter.WriteLine();
                        }

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
                                    exportWriter.Write(a.GetValue(j));

                                    if (!isEndOfArray)
                                        exportWriter.Write(",");
                                }
                            }
                            else
                            {
                                var value = field.GetValue(item);
                                if (value.GetType() == typeof(string))
                                    value = StringToCSVCell((string)value);

                                exportWriter.Write(value);
                            }

                            if (!isEndOfRecord)
                                exportWriter.Write(",");
                        }

                        exportWriter.WriteLine();
                    }

                    exportWriter.Dispose();

                    return new FileContentResult(exportStream.ToArray(), "application/octet-stream")
                    {
                        FileDownloadName = Path.ChangeExtension(name, ".csv")
                    };
                }
            }
            catch (FileNotFoundException e)
            {
                Logger.WriteLine("DBC " + name + " for build " + build + " not found: " + e.Message);
                return NotFound();
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error during CSV generation for DBC " + name + " for build " + build + ": " + e.Message);
                return BadRequest();
            }
        }

        public static string StringToCSVCell(string str)
        {
            var mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                var sb = new StringBuilder();
                sb.Append("\"");
                foreach (var nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }
    }
}
