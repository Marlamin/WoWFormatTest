using System.IO;
using System.Net.Http;

namespace ExporterCLI
{
    public static class CASC
    {
        private static string BuildConfig;
        private static string CDNConfig;
        private static HttpClient client;

        public static void InitCasc(string buildconfig, string cdnconfig)
        {
            BuildConfig = buildconfig;
            CDNConfig = cdnconfig;
            client = new HttpClient();
        }

        public static Stream OpenFile(string filename)
        {
            var response = client.GetAsync("http://localhost:5005/casc/file/fname?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filename=" + filename);
            return response.Result.Content.ReadAsStreamAsync().Result;
        }

        public static Stream OpenFile(int FileDataID)
        {
            var response = client.GetAsync("http://localhost:5005/casc/file/fdid?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filedataid=" + FileDataID + "&filename=" + FileDataID + ".wmo");
            return response.Result.Content.ReadAsStreamAsync().Result;
        }

        public static bool FileExists(string filename)
        {
            var response = client.GetAsync("http://localhost:5005/casc/file/fname?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filename=" + filename);
            return response.Result.IsSuccessStatusCode;
        }

        public static bool FileExists(int FileDataID)
        {
            var response = client.GetAsync("http://localhost:5005/casc/file/fname?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filedataid=" + FileDataID + "&filename=" + FileDataID + ".wmo");
            return response.Result.IsSuccessStatusCode;
        }
    }
}
