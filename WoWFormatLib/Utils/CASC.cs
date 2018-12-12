using System;
using System.IO;
using System.Net.Http;
using CASCLib;

namespace WoWFormatLib.Utils
{
    public class CASC
    {
        private static CASCHandler cascHandler;
        public static bool IsCASCInit = false;
        public static string BuildName;

        // Talking to local API instead of handling CASC via CASCLib
        private static bool usingLocalAPI;
        private static string CASCToolHostURL;
        private static string BuildConfig;
        private static string CDNConfig;
        private static HttpClient Client;

        public static void InitCasc(string cascToolHostURL, string buildConfig, string cdnConfig)
        {
            usingLocalAPI = true;
            CASCToolHostURL = cascToolHostURL;
            BuildConfig = buildConfig;
            CDNConfig = cdnConfig;
            Client = new HttpClient();

            IsCASCInit = true;
        }

        // Handle via CASCLib
        public static void InitCasc(BackgroundWorkerEx worker = null, string basedir = null, string program = "wowt", LocaleFlags locale = LocaleFlags.enUS){
            usingLocalAPI = false;
            CASCConfig.LoadFlags &= ~(LoadFlags.Download | LoadFlags.Install);
            CASCConfig.ValidateData = false;
            CASCConfig.ThrowOnFileNotFound = false;
            
            if (basedir == null)
            {
                Console.WriteLine("Initializing CASC from web for program " + program );
                cascHandler = CASCHandler.OpenOnlineStorage(program, "eu", worker);
            }
            else
            {
                basedir = basedir.Replace("_retail_", "").Replace("_ptr_", "");
                Console.WriteLine("Initializing CASC from local disk with basedir " + basedir);
                cascHandler = CASCHandler.OpenLocalStorage(basedir, worker);
            }

            BuildName = cascHandler.Config.BuildName;
            
            cascHandler.Root.SetFlags(locale, ContentFlags.None, false);

            IsCASCInit = true;
        }

        public static uint getFileDataIdByName(string filename)
        {
            if (usingLocalAPI)
            {
                var response = Client.GetStringAsync("http://" + CASCToolHostURL + "/casc/root/getfdid?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filename=" + filename);

                uint.TryParse(response.Result, out var filedataid);

                return filedataid;
            }
            else
            {
                return (uint)(cascHandler.Root as WowRootHandler)?.GetFileDataIdByName(filename);
            }
        }

        public static Stream OpenFile(string filename)
        {
            if (usingLocalAPI)
            {
                var response = Client.GetAsync("http://" + CASCToolHostURL + "/casc/file/fname?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filename=" + filename);
                return response.Result.Content.ReadAsStreamAsync().Result;
            }
            else
            {
                return cascHandler.OpenFile(filename);
            }
        }

        public static Stream OpenFile(uint filedataid)
        {
            if (usingLocalAPI)
            {
                var response = Client.GetAsync("http://" + CASCToolHostURL + "/casc/file/fdid?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filename=" + filedataid + "&filedataid=" + filedataid);
                return response.Result.Content.ReadAsStreamAsync().Result;
            }
            else
            {
                return cascHandler.OpenFile((int)filedataid);
            }
        }

        public static bool FileExists(string filename)
        {
            if (usingLocalAPI)
            {
                if(Client.GetStringAsync("http://" + CASCToolHostURL + "/casc/root/exists?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig + "&filename=" + filename).Result == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return cascHandler.FileExists(filename);
            }
        }

        public static bool FileExists(uint filedataid)
        {
            if (usingLocalAPI)
            {
                if (Client.GetStringAsync("http://" + CASCToolHostURL + "/casc/root/exists/" + filedataid + "?buildconfig=" + BuildConfig + "&cdnconfig=" + CDNConfig).Result == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return cascHandler.FileExists((int)filedataid);
            }
        }
    }
}
