using System.Net;
using FluentValidation;
using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Models;

namespace MarketDataProvider.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TickerDetailsController(
        IMemoryCache memoryCache,
        IAggregateRepository repository,
        IValidator<string> validator,
        ILogger<TickerDetailsController> logger) : ControllerBase
    {
        [HttpGet("{ticker}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleTickerDetailsRequest(string ticker)
        {
            var validationResult = validator.Validate(ticker);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return BadRequest(GenerateErrorResult(HttpStatusCode.BadRequest, errors));
            }

            try
            {
                var tickerDetails = memoryCache.Get<TickerDetails>($"TickerDetails_{ticker}");

                if (tickerDetails is not null)
                {
                    return Ok(tickerDetails);
                }

                var response = await repository.GetTickerDetailsAsync(ticker);

                if (response is null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                        GenerateErrorResult(HttpStatusCode.InternalServerError, "Invalid response."));
                }

                return Ok(response);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, GenerateErrorResult(HttpStatusCode.InternalServerError, "Internal server error."));
            }
        }

        private static OperationResult<TickerDetails> GenerateErrorResult(HttpStatusCode status, params string[] errors)
        {
            return new OperationResult<TickerDetails>
            {
                Status = status,
                ErrorMessages = errors
            };
        }
    }
}
