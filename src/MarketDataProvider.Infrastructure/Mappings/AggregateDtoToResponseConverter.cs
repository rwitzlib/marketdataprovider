using AutoMapper;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Responses;
using Polygon.Client.Models;

namespace MarketDataProvider.Infrastructure.Mappings
{
    public class AggregateDtoToResponseConverter : ITypeConverter<IEnumerable<AggregateDto>, AggregateResponse>
    {
        public AggregateResponse Convert(IEnumerable<AggregateDto> source, AggregateResponse destination, ResolutionContext context)
        {
            if (source is null || !source.Any())
            {
                return null;
            }

            var aggregates = source.ToArray();

            var candles = new List<Bar>();

            for (var i = 0; i < aggregates.Length; i++)
            {
                if (aggregates[i].Ticker is null)
                {
                    return null;
                }

                if (i > 0 && !aggregates[i].Ticker.Equals(aggregates[i - 1].Ticker))
                {
                    return null;
                }

                if (aggregates[i].Results is null)
                {
                    continue;
                }

                candles.AddRange(aggregates[i].Results);
            }

            return new AggregateResponse
            {
                Ticker = aggregates[0].Ticker,
                Results = candles,
            };
        }
    }
}
