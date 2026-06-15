using CrawlScope.Domain.Common;
using CrawlScope.Domain.Modules.Crawling.Enums;
using CrawlScope.Domain.Modules.Crawling.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlScope.Domain.Modules.Export.Models
{
    public class ExportFile : BaseEntity
    {
        public Guid CrawlJobId { get; set; }
        public CrawlJob CrawlJob { get; set; } = null!;
        public ExportFormat Format { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; } = null!;
    }
}
