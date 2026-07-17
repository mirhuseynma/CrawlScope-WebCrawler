namespace CrawlScope.Application.Modules.Crawling.Mappings
{
    public class CrawlingProfile : Profile
    {
        public CrawlingProfile()
        {
            CreateMap<CreateCrawlJobRequestDto, CrawlJob>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.CrawlType));
            CreateMap<CrawlJob, CrawlJobListItemDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<CrawlJob, CrawlJobDetailsDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
