using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;

namespace MarketDataProvider.Contracts.Interfaces
{
    public interface IAggregateInteraction
    {
        Task<OperationResult<AggregateResponse>> GetAggregateAsync(AggregateRequest request);
    }
}
