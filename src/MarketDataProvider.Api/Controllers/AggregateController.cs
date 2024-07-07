using System.Net;
using MarketDataProvider.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MarketDataProvider.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AggregateController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AggregateController> _logger;

        public AggregateController(ILogger<AggregateController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleAggregateRequest([FromBody] AggregateRequest request)
        {
            request.To = request.To.Date.AddDays(1).AddMinutes(-1);
            try
            {
                var response = await _mediator.Send(request);

                return response.Status switch
                {
                    HttpStatusCode.OK => Ok(response.Data),
                    HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
                    HttpStatusCode.NotFound => NotFound(response.ErrorMessages),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
            }
        }
    }
}
