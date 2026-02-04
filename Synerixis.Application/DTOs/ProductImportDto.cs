using Synerixis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class ProductImportDto
    {
        public string ExternalId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("imagesJson")]
        public string ImagesJson { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("tagsJson")]
        public string TagsJson { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}
