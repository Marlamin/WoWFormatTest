using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CASCtest
{
    class CascUtils
    {
        public static byte[] DownloadFileFromCDN(string path, string outputpath = "")
        {
            byte[] arr;
            //TODO path starts with data, check if file is present in local archives first, THEN web client
            using (WebClient client = new WebClient())
            {

                client.Headers["User-Agent"] = "Marlamin's CASC thing";
                if (outputpath.Count() > 0)
                {
                    try
                    {
                        client.DownloadFile("http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path, outputpath);
                    }
                    catch (System.Net.WebException e)
                    {
                        Console.WriteLine("[ERROR] 404 not found (" + "http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path + ")");
                    }
                    arr = new Byte[1];
                }
                else
                {
                    try
                    {
                        arr = client.DownloadData("http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path);
                    }
                    catch (System.Net.WebException e)
                    {
                        Console.WriteLine("[ERROR] 404 not found (" + "http://dist.blizzard.com.edgesuite.net/tpr/wow/" + path + ")");
                        arr = new Byte[1];
                    }
                }
            }
            return arr;
        }
    }
}
