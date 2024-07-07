using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;

namespace MarketDataProvider.Contracts.Interfaces
{
    public interface IScanInteraction
    {
        public Task<OperationResult<ScanPopulateResponse>> ScanAsync(ScanPopulateRequest request);
    }
}
