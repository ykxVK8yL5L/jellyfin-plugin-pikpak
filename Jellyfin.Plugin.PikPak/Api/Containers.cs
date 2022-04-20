using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.PikPak.Api.Containers
{
    public class File
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("file_extension")]
        public string? FileExtension { get; set; }

        [JsonPropertyName("thumbnail_link")]
        public string? ThumbnailLink { get; set; }

        [JsonPropertyName("original_url")]
        public string? OriginalUrl { get; set; }

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }



    }

  

    
}