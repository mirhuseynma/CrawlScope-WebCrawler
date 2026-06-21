using AutoMapper;
using CrawlScope.Application.Modules.Crawling.DTOs;
using CrawlScope.Domain.Modules.Crawling.Models;

namespace CrawlScope.Application.Modules.Crawling.Mappings
{
    public class CrawlingProfile : Profile
    {
        public CrawlingProfile()
        {
            CreateMap<CreateCrawlJobRequestDto, CrawlJob>();
            CreateMap<CrawlJob, CrawlJobListItemDto>().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CrawlJob, CrawlJobDetailsDto>().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
