using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.PikPak.Configuration;
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
        private string _token;
        private readonly JsonSerializerOptions _jsonSerializerOptions = JsonDefaults.GetOptions();

        public PikPakApi()
        {
            _username = Plugin.Instance.Configuration.UserName;
            _password = Plugin.Instance.Configuration.Password;
            _proxy_url = Plugin.Instance.Configuration.ProxyUrl;
            _token = Plugin.Instance.Configuration.Token;
        }

        public async void TokenRefresh()
        {

            var url =  "https://user.mypikpak.com/v1/auth/signin";
            if(_proxy_url.Length>4){
                url = _proxy_url+"/"+url;
            }
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                {
                    request.Content = new StringContent("{\"captcha_token\":\"\",\"client_id\":\"YNxT9w7GMdWvEOKa\",\"client_secret\":\"dbw2OtmVEeuUvIptb1Coyg\",\"username\":\""+_username+"\",\"password\":\""+_password+"\"}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain"); 
                    var response = await httpClient.SendAsync(request);
                    string result = response.Content.ReadAsStringAsync().Result;


                    var response_obj = JObject.Parse(result);
                    if(response_obj.ContainsKey("access_token")){
                        var access_token = response_obj["access_token"].ToString();
                        if(!string.IsNullOrEmpty(access_token)){
                            _token = access_token;
                            Plugin.Instance.Configuration.Token = _token;
                            Plugin.Instance.SaveConfiguration();
                        }
                    }

                }
            }
            
        }

        public async Task<string> GetFileListAsync(string folder_id, string pagetoken)
        {
            var response_content = "";
            var url = "https://api-drive.mypikpak.com/drive/v1/files?parent_id="+folder_id+"&page_token="+pagetoken+"&thumbnail_size=SIZE_LARGE&with_audit=true&filters=%7B%22phase%22:%7B%22eq%22:%22PHASE_TYPE_COMPLETE%22%7D,%22trashed%22:%7B%22eq%22:false%7D%7D";
             if(_proxy_url.Length>4){
                url = _proxy_url+"/"+url;
            }
            
            
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    var response = await httpClient.SendAsync(request);
                    if ((int)response.StatusCode == 401){
                        TokenRefresh();
                        return await GetFileListAsync(folder_id, pagetoken);
                    }
                    string result = response.Content.ReadAsStringAsync().Result;
                    response_content = result;
                    // return result;
                }
                    
            }
            return response_content;

        }


        public async Task<string> GetFileInfoAsync(string file_id)
        {
            var response_content = "";
            var url = "https://api-drive.mypikpak.com/drive/v1/files/"+file_id+"?thumbnail_size=SIZE_LARGE";
            if(_proxy_url.Length>4){
                url = _proxy_url+"/"+url;
            }

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    //request.Headers.User-Agent = new AuthenticationHeaderValue("Bearer", _token);
                    request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");
                    request.Headers.Add("Origin", "https://api-drive.mypikpak.com/drive/v1/files");
                    request.Headers.Add("Referer", "https://api-drive.mypikpak.com/drive/v1/files");
                    request.Headers.Add("Authorization", "Bearer " + _token);

                    var response = await httpClient.SendAsync(request);
                    if ((int)response.StatusCode == 401){
                        TokenRefresh();
                        return await GetFileInfoAsync(file_id);
                    }
                    string result = response.Content.ReadAsStringAsync().Result;
                    response_content = result;
                }
                    
            }
            return response_content;

        }


        


    }
}