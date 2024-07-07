using AutoFixture;
using FluentAssertions;
using MarketDataProvider.Api.Controllers;
using MarketDataProvider.Application.Handlers;
using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using MarketDataProvider.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MarketDataProvider.Api.UnitTests.Controllers
{
    public class AggregateControllerUnitTests
    {
        private readonly AggregateController _classUnderTest;
        private readonly Fixture _autofixture;
        private readonly AutoMocker _autoMocker;

        public AggregateControllerUnitTests()
        {
            _autoMocker = new AutoMocker();
            _autofixture = new Fixture();

            _classUnderTest = _autoMocker.CreateInstance<AggregateController>();
        }

        [Fact]
        public async Task Valid_Response_With_Aggregate_Returns_OK_Response()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            GivenHandlerReturnsValidResponse();

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = response.Should().BeOfType<OkObjectResult>().Subject;
            result.Value.Should().BeOfType<AggregateResponse>();
        }

        [Fact]
        public async Task BadRequest_Response_With_Aggregate_Returns_BadRequest_Response()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            GivenHandlerReturnsErrorResponse(HttpStatusCode.BadRequest);

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = response.Should().BeOfType<BadRequestObjectResult>().Subject;
            result.Value.Should().NotBeOfType<AggregateResponse>();
        }


        [Fact]
        public async Task NotFound_Response_With_Aggregate_Returns_NotFound_Response()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            GivenHandlerReturnsErrorResponse(HttpStatusCode.NotFound);

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = response.Should().BeOfType<NotFoundObjectResult>().Subject;
            result.Value.Should().NotBeOfType<AggregateResponse>();
        }

        [Theory]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task Error_Response_With_Aggregate_Returns_InternalServerError_Response(HttpStatusCode status)
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            GivenHandlerReturnsErrorResponse(status);

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = response.Should().BeOfType<ObjectResult>().Subject;
            result.Value.Should().NotBeOfType<AggregateResponse>();
        }

        [Fact]
        public async Task Thrown_Exception_Returns_InternalServerError_Response()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            GivenHandlerThrowsException();

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = response.Should().BeOfType<ObjectResult>().Subject;
            result.Value.Should().NotBeOfType<AggregateResponse>();
        }


        private void GivenHandlerReturnsValidResponse()
        {
            var response = new OperationResult<AggregateResponse>
            {
                Status = HttpStatusCode.OK,
                Data = _autofixture.Create<AggregateResponse>()
            };

            _autoMocker.GetMock<IMediator>()
                .Setup(method => method.Send(It.IsAny<AggregateRequest>(), default))
                .ReturnsAsync(response);
        }

        private void GivenHandlerReturnsErrorResponse(HttpStatusCode status)
        {
            var response = new OperationResult<AggregateResponse>
            {
                Status = status,
                ErrorMessages = _autofixture.CreateMany<string>()
            };

            _autoMocker.GetMock<IMediator>()
                .Setup(method => method.Send(It.IsAny<AggregateRequest>(), default))
                .ReturnsAsync(response);
        }

        private void GivenHandlerThrowsException()
        {
            _autoMocker.GetMock<IMediator>()
                .Setup(method => method.Send(It.IsAny<AggregateRequest>(), default))
                .ThrowsAsync(new Exception());
        }
    }
}
