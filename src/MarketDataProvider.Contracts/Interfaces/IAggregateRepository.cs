using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using Polygon.Client.Models;

namespace MarketDataProvider.Core.Interfaces
{
    public interface IAggregateRepository
    {
        public Task<AggregateResponse> QueryAggregateAsync(AggregateRequest request);
        public Task<IEnumerable<string>> ScanAggregatesAsync(ScanPopulateRequest request);
        public Task<TickerDetails> GetTickerDetailsAsync(string ticker);
    }
}
