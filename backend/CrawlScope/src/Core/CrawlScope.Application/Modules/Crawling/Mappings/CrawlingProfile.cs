using AutoMapper;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Domain.Modules.Crawling.Models;

namespace CrawlScope.Application.Modules.Crawling.Mappings
{
    public class CrawlingProfile : Profile
    {
        public CrawlingProfile()
        {
            CreateMap<CreateCrawlJobRequest, CrawlJob>();
        }
    }
}
