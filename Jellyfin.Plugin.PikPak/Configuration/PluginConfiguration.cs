using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PikPak.Configuration
{
      public class PluginConfiguration : BasePluginConfiguration
    {
        // store configurable settings your plugin might need
        public int CacheSeconds { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ProxyUrl { get; set; }
       

        public PluginConfiguration()
        {
            // set default options here
           CacheSeconds = 0;
           UserName = "";
           Password = "";
           ProxyUrl = "";
        }
    }
}
