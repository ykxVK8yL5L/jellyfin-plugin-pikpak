using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.PikPak.Configuration;
using Jellyfin.Plugin.PikPak.Api.Containers;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PikPak.Api
{
 
    public class PikPakApi
    {
        private const string AuthLink = "https://api.pikpak.com/v1/auth";
        private const string FileLink = "https://api.pikpak.com/v1/file";
        private readonly string _username;
        private readonly string _password;
        private readonly string _proxy_url;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PikPakApi> _logger;
        private readonly string _token;
        private readonly JsonSerializerOptions _jsonSerializerOptions = JsonDefaults.GetOptions();
        private readonly HttpClient _httpClient;

        public PikPakApi(IHttpClientFactory httpClientFactory,ILogger<PikPakApi> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient =  _httpClientFactory.CreateClient("PikPakApi");
            _logger = logger;
            _username = Plugin.Instance.Configuration.UserName;
            _password = Plugin.Instance.Configuration.Password;
            _proxy_url = Plugin.Instance.Configuration.ProxyUrl;
            _token = Plugin.Instance.Configuration.Token;

        }

        public async Task<string> GetFileListAsync(string folder_id)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            var httpResponseMessage = await _httpClient.GetAsync("repos/dotnet/AspNetCore.Docs/branches");
            var responseValue = string.Empty;

            if (httpResponseMessage.IsSuccessStatusCode)
            {

                Task task = httpResponseMessage.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    var stream = t.Result;
                    using (var reader = new StreamReader(stream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                });
                task.Wait();

            }
            return responseValue;
        }

       
   
    }
}