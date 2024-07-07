using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MarketDataProvider.Application.Handlers;
using MarketDataProvider.Application.Validators;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using MarketDataProvider.Core.Interfaces;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace MarketDataProvider.Application.UnitTests.Handlers
{
    public class AggregateHandlerUnitTests
    {
        private readonly AggregateHandler _classUnderTest;
        private readonly Fixture _autofixture;
        private readonly AutoMocker _autoMocker;

        public AggregateHandlerUnitTests()
        {
            _autoMocker = new AutoMocker();
            _autofixture = new Fixture();

            _classUnderTest = _autoMocker.CreateInstance<AggregateHandler>();
        }

        [Fact]
        public async Task Handle_With_Valid_Repository_Response_Returns_Valid_Response()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            request.Ticker = "SPY";
            GivenValidatorReturnsValidResult();
            GivenRepositoryReturnsValidResponse();

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.ErrorMessages.Should().BeNullOrEmpty();
            response.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_With_Null_Repository_Response_Returns_Error_Response()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            request.Ticker = "SPY";
            GivenValidatorReturnsValidResult();

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.ErrorMessages.Should().NotBeNullOrEmpty();
            response.Data.Should().BeNull();
        }

        [Fact]
        public async Task Handle_With_Invalid_Aggregate_Request_Returns_Error_Response()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            request.Ticker = "asdf12";
            GivenValidatorReturnsErrorResult();

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.ErrorMessages.Should().NotBeNullOrEmpty();
            response.Data.Should().BeNull();
        }

        private void GivenValidatorReturnsValidResult()
        {
            _autoMocker.GetMock<IValidator<AggregateRequest>>()
                .Setup(method => method.Validate(It.IsAny<AggregateRequest>()))
                .Returns(new ValidationResult
                {
                    Errors = Enumerable.Empty<ValidationFailure>().ToList()
                });
        }

        private void GivenValidatorReturnsErrorResult()
        {
            var errors = _autofixture.Create<List<ValidationFailure>>();
            _autoMocker.GetMock<IValidator<AggregateRequest>>()
                .Setup(method => method.Validate(It.IsAny<AggregateRequest>()))
                .Returns(new ValidationResult
                {
                    Errors = errors
                });
        }

        private void GivenRepositoryReturnsValidResponse()
        {
            var response = _autofixture.Create<AggregateResponse>();
            _autoMocker.GetMock<IAggregateRepository>()
                .Setup(method => method.QueryAggregateAsync(It.IsAny<AggregateRequest>()))
                .ReturnsAsync(response);
        }
    }
}
