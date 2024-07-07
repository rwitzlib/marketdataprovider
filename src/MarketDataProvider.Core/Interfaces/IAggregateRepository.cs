using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataProvider.Core.Interfaces
{
    public interface IAggregateRepository
    {
        Task<AggregateResponseDto> RetrieveAggregateData(AggregateRequestDto request);
    }
}
