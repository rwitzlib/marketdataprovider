using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Requests;
using Polygon.Client.Models;

namespace MarketDataProvider.Clients.Interfaces
{
    public interface IMarketCacheClient
    {
        public Task<IEnumerable<TAggregateType>> QueryAggregates<TAggregateType>(AggregateRequest request);
        public Task<IEnumerable<TAggregateType>> ScanAggregates<TAggregateType>(DateTime date);
        public Task<TickerDetails> GetTickerDetails(string ticker);
        public Task SetAggregate(string prefix, AggregateDto aggregate, int expiresInMinutes = -1);
        public Task<TickerDetails> SetTickerDetails(string prefix, TickerDetails tickerDetails, int expiresInMinutes = -1);
    }
}
