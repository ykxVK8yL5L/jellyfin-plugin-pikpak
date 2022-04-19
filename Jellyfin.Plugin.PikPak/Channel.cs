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
/// using Newtonsoft.Json.Linq;

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
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
        public Channel(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Channel>();
            _logger.LogInformation("PikPak channel created");
            var pikpakApiLogger = loggerFactory.CreateLogger<PikPakApi>();
            _pikPakApi = new PikPakApi(httpClientFactory, pikpakApiLogger);
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
            _logger.LogDebug("[PikPak][GetFolders] Get Sport Folders");
            var pagetoken = "";
            var fileList = new List<File>();
            _logger.LogInformation("fuckGetFolders");
            while(true){
                _logger.LogInformation("fuckresponse_body");
                var response_body = await _pikPakApi.GetFileListAsync(folder_id,pagetoken).ConfigureAwait(false);
                 _logger.LogInformation(response_body);
                break;
                
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
                    Type = file.Kind=="drive#folder"?ChannelItemType.Folder:ChannelItemType.Media,
                }).ToList(),
                TotalRecordCount = fileList.Count
            };
        }

    
        /// <inheritdoc />
        public async Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
        {
            var split = id.Split('_', StringSplitOptions.RemoveEmptyEntries);
           
            return Enumerable.Empty<MediaSourceInfo>();

            // return new List<MediaSourceInfo>
            // {
            //     new ()
            //     {
            //         Path = streamUrl,
            //         Protocol = MediaProtocol.Http,
            //         Id = id,
            //         Bitrate = bitrate,
            //         SupportsProbing = false
            //     }
            // };
        }

    }
}