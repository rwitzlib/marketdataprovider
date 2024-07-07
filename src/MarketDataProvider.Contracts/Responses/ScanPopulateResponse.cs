using System.Diagnostics.CodeAnalysis;

namespace MarketDataProvider.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    public class ScanPopulateResponse
    {
        public int Count { get; set; }
        public IEnumerable<string> Tickers { get; set; }
    }
}
