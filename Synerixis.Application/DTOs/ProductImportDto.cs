using Synerixis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class ProductImportDto
    {
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string ImagesJson { get; set; }
        public Category Category { get; set; }
        public string TagsJson { get; set; }
        public string Source { get; set; }
    }
}
