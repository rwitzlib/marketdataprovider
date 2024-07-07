using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace MarketDataProvider.Contracts.Models
{
    [ExcludeFromCodeCoverage]
    public class OperationResult<TType>
    {
        public HttpStatusCode Status { get; set; }
        public TType Data { get; set; }
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}