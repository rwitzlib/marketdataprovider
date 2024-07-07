using AutoMapper;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using Polygon.Client.Requests;
using Polygon.Client.Responses;

namespace MarketDataProvider.Infrastructure.Mappings
{
    public class AggregateProfile : Profile
    {
        public AggregateProfile()
        {
            CreateMap<AggregateRequest, PolygonAggregateRequest>()
                .ForMember(dest => dest.Ticker, opt => opt.MapFrom(src => src.Ticker.ToUpperInvariant()))
                .ForMember(dest => dest.Timespan, opt => opt.MapFrom(src => src.Timespan.ToString().ToLowerInvariant()))
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.From.ToUnixTimeMilliseconds()))
                .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.To.ToUnixTimeMilliseconds()))
                .ForMember(dest => dest.Adjusted, opt => opt.Ignore())
                .ForMember(dest => dest.Sort, opt => opt.Ignore())
                .ForMember(dest => dest.Limit, opt => opt.Ignore());

            CreateMap<PolygonAggregateResponse, AggregateResponse>();
            //.ForMember(q => q.Studies, opt => opt.Ignore())
            //.ForMember(q => q.TickerDetails, opt => opt.Ignore());

            CreateMap<PolygonAggregateResponse, AggregateDto>()
                .ForMember(dest => dest.Date, opt => 
                    opt.MapFrom(src => int.Parse(DateTimeOffset.FromUnixTimeMilliseconds(src.Results.Last().Timestamp).ToString("yyyyMMdd"))))
                .ForMember(dest => dest.Count, opt => opt.MapFrom(dest => dest.ResultsCount));

            CreateMap<PolygonAggregateResponse, IEnumerable<AggregateDto>>()
                .ConvertUsing<PolygonAggregateToDtoConverter>();

            CreateMap<IEnumerable<AggregateDto>, AggregateResponse>()
                .ConvertUsing<AggregateDtoToResponseConverter>();

            CreateMap<AggregateResponse, AggregateDto>()
                .ForMember(dest => dest.Date, opt => 
                    opt.MapFrom(src => DateTimeOffset.FromUnixTimeMilliseconds(src.Results.First().Timestamp).Date.ToString("yyyyMMdd")))
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Results.Count))
                .ReverseMap();
        }
    }
}
