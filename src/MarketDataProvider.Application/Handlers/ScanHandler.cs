using FluentValidation;
using MarketDataProvider.Contracts.Interfaces;
using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using MarketDataProvider.Core.Interfaces;
using MediatR;
using System.Net;

namespace MarketDataProvider.Application.Handlers
{
    public class ScanHandler(
        IValidator<ScanPopulateRequest> validator,
        IAggregateRepository aggregateRepository) : IScanInteraction, IRequestHandler<ScanPopulateRequest, OperationResult<ScanPopulateResponse>>
    {
        public async Task<OperationResult<ScanPopulateResponse>> Handle(ScanPopulateRequest request, CancellationToken cancellationToken)
            => await ScanAsync(request);

        public async Task<OperationResult<ScanPopulateResponse>> ScanAsync(ScanPopulateRequest request)
        {
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                GenerateErrorResult(HttpStatusCode.BadRequest, errors);
            }

            var tickers = await aggregateRepository.ScanAggregatesAsync(request);

            if (tickers is null || !tickers.Any())
            {
                return GenerateErrorResult(HttpStatusCode.NotFound, "No data found.");
            }

            return new OperationResult<ScanPopulateResponse>
            {
                Status = HttpStatusCode.OK,
                Data = new ScanPopulateResponse
                {
                    Count = tickers.Count(),
                    Tickers = tickers
                }
            };
        }

        private static OperationResult<ScanPopulateResponse> GenerateErrorResult(HttpStatusCode status, params string[] errors)
        {
            return new OperationResult<ScanPopulateResponse>
            {
                Status = status,
                ErrorMessages = errors
            };
        }
    }
}
