using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MarketDataProvider.Contracts.Enums;
using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Responses;
using MediatR;

namespace MarketDataProvider.Contracts.Requests
{
    [ExcludeFromCodeCoverage]
    public class AggregateRequest : IRequest<OperationResult<AggregateResponse>>
    {
        /// <summary>
        /// The ticker symbol of the stock/equity.
        /// </summary>
        [Required]
        [StringLength(6)]
        public string Ticker { get; set; }

        /// <summary>
        /// The size of the timespan multiplier.
        /// </summary>
        [Required]
        [Range(0, 10)]
        public int Multiplier { get; set; }

        /// <summary>
        /// The size of the time window.
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Timespan Timespan { get; set; }

        /// <summary>
        /// The start of the aggregate time window. Either a date with the format YYYY-MM-DD or 
        /// a millisecond timestamp.
        /// </summary>
        [Required]
        public DateTimeOffset From { get; set; }

        /// <summary>
        /// The end of the aggregate time window. Either a date with the format YYYY-MM-DD or 
        /// a millisecond timestamp.
        /// </summary>
        [Required]
        public DateTimeOffset To { get; set; }
    }
}
