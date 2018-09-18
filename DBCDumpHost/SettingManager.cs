using System.IO;
using Microsoft.Extensions.Configuration;

namespace DBCDumpHost
{
    public static class SettingManager
    {
        public static string definitionDir;
        public static string dbcDir;

        static SettingManager()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json", optional: false, reloadOnChange: false).Build();
            definitionDir = config.GetSection("config")["definitionsdir"];
            dbcDir = config.GetSection("config")["dbcdir"];
        }
    }
}
