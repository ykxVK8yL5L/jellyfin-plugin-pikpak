using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.PikPak.Configuration;
using Jellyfin.Plugin.PikPak.Api;
using Jellyfin.Plugin.PikPak.Api.Containers;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.PikPak
{
    /// <summary>
    /// The PikPak channel.
    /// </summary>
    public class Channel : IChannel, IRequiresMediaInfoCallback
    {
        /// private static readonly double CacheExpireTime = TimeSpan.FromSeconds(60).TotalMilliseconds;
        private readonly ILogger<Channel> _logger;
        private readonly PikPakApi _pikPakApi;
  
        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
        public Channel(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Channel>();
            _logger.LogInformation("PikPak channel created");
            var pikpakApiLogger = loggerFactory.CreateLogger<PikPakApi>();
            _pikPakApi = new PikPakApi(pikpakApiLogger);
        }

        /// <inheritdoc />
        public string Name => Plugin.Instance!.Name;

        /// <inheritdoc />
        public string Description => Plugin.Instance!.Description;

        /// <inheritdoc />
        public string DataVersion => "5";

        /// <inheritdoc />
        public string HomePageUrl => "https://hub.docker.com/r/ykxvk8yl5l/pikpak-webdav";

        /// <inheritdoc />
        public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

        /// <inheritdoc />
        public bool IsEnabledFor(string userId) => true;

        /// <inheritdoc />
        public InternalChannelFeatures GetChannelFeatures()
        {
            return new ()
            {
                MaxPageSize = 50,
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.TvExtra
                },
                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                }
            };
        }

        /// <inheritdoc />
        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[PikPak] GetChannelImage {ImagePath}", GetType().Namespace + ".Images.PikPak.png");
            _logger.LogInformation("[PikPak] GetChannelImage {ImagePath}", GetType().Namespace + ".Images.PikPak.png");
            var path = GetType().Namespace + ".Images.PikPak.png";
            return Task.FromResult(new DynamicImageResponse
            {
                Format = ImageFormat.Png,
                HasImage = true,
                Stream = GetType().Assembly.GetManifestResourceStream(path)
            });
        }

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return Enum.GetValues(typeof(ImageType)).Cast<ImageType>();
        }

        /// <inheritdoc />
        public Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[PikPak][GetChannelItems] Searching ID: {FolderId}", query.FolderId);
            return GetFolders(query.FolderId);
        }


        /// <summary>
        ///  Return list of file folders
        /// </summary>
        /// <returns>The channel item result.</returns>
        private async Task<ChannelItemResult> GetFolders(string folder_id)
        {
            _logger.LogInformation("[PikPak][GetFolders] Get Folders");
            string pagetoken = string.Empty;
            List<File> fileList = new List<File>();
            while(true){
                var response_body = await _pikPakApi.GetFileListAsync(folder_id,pagetoken).ConfigureAwait(false);
                var response_obj = JObject.Parse(response_body);
                foreach(var file in response_obj["files"]){
                    var file_obj = JObject.Parse(file.ToString());
                    if(string.Equals(file_obj["kind"].ToString(),"drive#file") && !file_obj["mime_type"].ToString().Contains("video")){
                        continue;
                    }
                    fileList.Add(new File
                    {
                        Id = file_obj["id"].ToString(),
                        Kind = file_obj["kind"].ToString(),
                        Name = file_obj["name"].ToString(),
                        FileExtension = file_obj["file_extension"].ToString(),
                        ThumbnailLink = file_obj["thumbnail_link"].ToString(),
                        OriginalUrl = file_obj["original_url"].ToString(),
                        MimeType = file_obj["mime_type"].ToString()
                    });
                }
                var next_page_token = response_obj["next_page_token"].ToString();
                pagetoken  = next_page_token;  
                if(string.IsNullOrEmpty(next_page_token)){
                    break;
                }
            }
            if (fileList.Count<1)
            {
                return new ChannelItemResult();
            }
           return new ChannelItemResult
            {
                Items = fileList.Select(file => new ChannelItemInfo
                {
                    Id = $"{file.Id}",
                    Name = $"{file.Name}",
                    ImageUrl = $"{file.ThumbnailLink}",
                    Type = file.Kind=="drive#folder"?ChannelItemType.Folder:ChannelItemType.Media,
                }).ToList(),
                TotalRecordCount = fileList.Count
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
        {
            //var split = id.Split('_', StringSplitOptions.RemoveEmptyEntries);
            _logger.LogInformation("[PikPak][GetFolders] GetChannelItemMediaInfo is"+id);

            var media_list = new List<MediaSourceInfo>();

            var response_body = await _pikPakApi.GetFileInfoAsync(id).ConfigureAwait(false);
            var response_obj = JObject.Parse(response_body);
            foreach(var file in response_obj["medias"]){
                var file_obj = JObject.Parse(file.ToString());
                media_list.Add(new MediaSourceInfo
                {
                    Name = file_obj["name"].ToString(),
                    Path = file_obj["link"]["url"].ToString(),
                    Protocol = MediaProtocol.Http,
                    Id = file_obj["media_id"].ToString(),
                    Bitrate = Int32.Parse(file_obj["video"]["bit_rate"].ToString()),
                    IsRemote = true,
                  
                });
            }
        

            // if (media_list.Count<1)
            // {
            //     return Enumerable.Empty<MediaSourceInfo>();
            // }
            

            return media_list;
        }

    }
}