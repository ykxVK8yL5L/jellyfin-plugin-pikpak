using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.PikPak.Configuration;
using Jellyfin.Plugin.PikPak.Api.Containers;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.PikPak.Api
{
 
    public class PikPakApi
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _proxy_url;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PikPakApi> _logger;
        private string _token;
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

        public async Task<string> GetFileListAsync(string folder_id, string pagetoken)
        {
            _logger.LogInformation("fuckApiGetFileListAsync");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            var url = "https://api-drive.mypikpak.com/drive/v1/files?parent_id="+folder_id+"&thumbnail_size=SIZE_LARGE&with_audit=true&page_token="+pagetoken+"&limit=0&filters={{\"phase\":{{\"eq\":\"PHASE_TYPE_COMPLETE\"}},\"trashed\":{{\"eq\":false}}}}";
            var httpResponseMessage = await _httpClient.GetAsync(url);
            var responseValue = string.Empty;

            if ((int)httpResponseMessage.StatusCode == 401){
                await RefreshToken();
                GetFileListAsync(folder_id, pagetoken);
            }
            
            if (httpResponseMessage.IsSuccessStatusCode)
            {

                _logger.LogInformation("fuckhttpResponseMessageIsSuccessStatusCode");
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


        public async Task RefreshToken(){
            var url = "https://user.mypikpak.com/v1/auth/signin";
            var data = new StringContent("{\"captcha_token\":\"\",\"client_id\":\"YNxT9w7GMdWvEOKa\",\"client_secret\":\"dbw2OtmVEeuUvIptb1Coyg\",\"username\":\""+_username+"\",\"password\":\""+_password+"\"}",Encoding.UTF8, "application/json");

            var httpResponseMessage = await _httpClient.PostAsync(url,data);
            var responseValue = string.Empty;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                _logger.LogInformation("fuckhttpResponseMessageIsSuccessStatusCode");
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
            JObject response_obj = JObject.Parse(responseValue);
            var access_token = response_obj["access_token"].ToString();
            if(!string.IsNullOrEmpty(access_token)){
                _logger.LogInformation(access_token);
                _token = access_token;
                Plugin.Instance.Configuration.Token = _token;
                Plugin.Instance.SaveConfiguration();
            }
  
        }



    }
}