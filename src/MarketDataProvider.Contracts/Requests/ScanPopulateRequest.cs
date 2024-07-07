using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Responses;
using MediatR;

namespace MarketDataProvider.Contracts.Requests
{
    public class ScanPopulateRequest : IRequest<OperationResult<ScanPopulateResponse>>
    {
        public DateTime Timestamp { get; set; }
    }
}
