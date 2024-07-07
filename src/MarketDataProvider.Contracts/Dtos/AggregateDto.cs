using Polygon.Client.Models;
using System.Diagnostics.CodeAnalysis;

namespace MarketDataProvider.Contracts.Dtos
{
    [ExcludeFromCodeCoverage]
    public class AggregateDto
    {
        public string Ticker { get; set; }
        public int Date { get; set; }
        public List<Bar> Results { get; set; }
        public int Count { get; set; }
    }
}
