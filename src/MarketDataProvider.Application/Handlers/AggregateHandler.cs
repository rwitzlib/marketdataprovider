using FluentValidation;
using MarketDataProvider.Contracts.Interfaces;
using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using MarketDataProvider.Core.Interfaces;
using MediatR;
using System.Net;

namespace MarketDataProvider.Application.Handlers;

public class AggregateHandler : IAggregateInteraction, IRequestHandler<AggregateRequest, OperationResult<AggregateResponse>>
{
    private readonly IValidator<AggregateRequest> _validator;
    private readonly IAggregateRepository _aggregateRepository;

    public AggregateHandler(IValidator<AggregateRequest> validator, IAggregateRepository aggregateRepository)
    {
        _validator = validator;
        _aggregateRepository = aggregateRepository;
    }

    public async Task<OperationResult<AggregateResponse>> Handle(AggregateRequest request, CancellationToken cancellationToken)
        => await GetAggregateAsync(request);

    public async Task<OperationResult<AggregateResponse>> GetAggregateAsync(AggregateRequest request)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(q => q.ErrorMessage).ToArray();
            return GenerateErrorResult(HttpStatusCode.BadRequest, errorMessages);
        }

        var aggregateResponse = await _aggregateRepository.QueryAggregateAsync(request);

        if (aggregateResponse is null)
        {
            return GenerateErrorResult(HttpStatusCode.NotFound, "No data found.");
        }

        return new OperationResult<AggregateResponse>
        {
            Status = HttpStatusCode.OK,
            Data = aggregateResponse
        };
    }

    private static OperationResult<AggregateResponse> GenerateErrorResult(HttpStatusCode status, params string[] errors)
    {
        return new OperationResult<AggregateResponse>
        {
            Status = status,
            ErrorMessages = errors
        };
    }
}

