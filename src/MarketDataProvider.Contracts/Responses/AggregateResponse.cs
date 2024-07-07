using System.Diagnostics.CodeAnalysis;
using Polygon.Client.Models;

namespace MarketDataProvider.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    public class AggregateResponse
    {
        /// <summary>
        /// The exchange symbol that this item is traded under.
        /// </summary>
        public string Ticker { get; set; }

        public List<Bar> Results { get; set; }
    }
}
