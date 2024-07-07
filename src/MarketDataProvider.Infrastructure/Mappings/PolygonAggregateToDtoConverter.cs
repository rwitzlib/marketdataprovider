using AutoMapper;
using MarketDataProvider.Contracts.Dtos;
using Polygon.Client.Responses;

namespace MarketDataProvider.Infrastructure.Mappings
{
    public class PolygonAggregateToDtoConverter : ITypeConverter<PolygonAggregateResponse, IEnumerable<AggregateDto>>
    {
        public IEnumerable<AggregateDto> Convert(PolygonAggregateResponse source, IEnumerable<AggregateDto> destination, ResolutionContext context)
        {
            if (source is null || source.Results is null || !source.Results.Any())
            {
                return [];
            }

            var startDate = DateTimeOffset.FromUnixTimeMilliseconds(source.Results.First().Timestamp);
            var endDate = DateTimeOffset.FromUnixTimeMilliseconds(source.Results.Last().Timestamp);

            var days = DateUtilities.GetMarketOpenDays(startDate, endDate);

            var aggregateDtos = new List<AggregateDto>();

            // TODO: could make this more efficient by tracking current index - starting there on each loop iteration
            foreach (var day in days)
            {
                var candles = source.Results.Where(candle => DateTimeOffset.FromUnixTimeMilliseconds(candle.Timestamp).Date == day).ToList();

                aggregateDtos.Add(new AggregateDto
                {
                    Ticker = source.Ticker,
                    Date = int.Parse(day.ToString("yyyyMMdd")),
                    Results = candles,
                    Count = candles.Count
                });
            }

            return aggregateDtos;
        }
    }
}
