using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.PikPak.Configuration;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PikPak
{
    /// <summary>
    /// The PikPak channel.
    /// </summary>
    public class Channel : IChannel, IRequiresMediaInfoCallback
    {
        /// private static readonly double CacheExpireTime = TimeSpan.FromSeconds(60).TotalMilliseconds;
        private readonly ILogger<Channel> _logger;
  
        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
        /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
        public Channel(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Channel>();
            _logger.LogInformation("PikPak channel created");

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

            /*
             *    id: {sport}_{date}_{gameId}_{network}_{quality}
             */

            /*
             *    Structure:
             *         Sport
             *             Date - Past 7 days?
             *                 Game Id
             *                     Home vs Away
             *                         Network - (Home/Away/3-Camera)
             *                             Quality
             */


            return GetFolders();

            // At root, return Folders
            // if (string.IsNullOrEmpty(query.FolderId))
            // {
            //     return GetFolders();
            // }

            // _logger.LogDebug("[PikPak][GetChannelItems] Current Search Key: {FolderId}", query.FolderId);

            // // Split parts to see how deep we are
            // var querySplit = query.FolderId.Split('_', StringSplitOptions.RemoveEmptyEntries);

            // switch (querySplit.Length)
            // {
            //     case 0:
            //         // List Folders
            //         return GetSportFolders();
            //     case 1:
            //         // List dates
            //         return GetDateFolders(querySplit[0]);
            //     case 2:
            //         // List games
            //         return GetGameFolders(querySplit[0], querySplit[1]);
            //     case 3:
            //         // List feeds
            //         return GetFeedFolders(querySplit[0], querySplit[1], int.Parse(querySplit[2], CultureInfo.InvariantCulture));
            //     case 4:
            //         // List qualities
            //         return GetQualityItems(querySplit[0], querySplit[1], int.Parse(querySplit[2], CultureInfo.InvariantCulture), querySplit[3]);
            //     default:
            //         // Unknown, return empty result
            //         return Task.FromResult(new ChannelItemResult());
            // }
        }

     

        /// <summary>
        ///  Return list of file folders
        /// </summary>
        /// <returns>The channel item result.</returns>
        private Task<ChannelItemResult> GetFolders()
        {
            _logger.LogDebug("[PikPak][GetSportFolders] Get Sport Folders");

            var info = new List<ChannelItemInfo>();

            info.Add(new ChannelItemInfo
            {
                Id = "nhl",
                Name = "NHL",
                Type = ChannelItemType.Folder
            });

            info.Add(new ChannelItemInfo
            {
                Id = "MLB",
                Name = "MLB",
                Type = ChannelItemType.Folder
            });

            return Task.FromResult(new ChannelItemResult
            {
                Items = info,
                TotalRecordCount = info.Count
            });
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