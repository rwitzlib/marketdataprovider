using AutoFixture;
using FluentAssertions;
using MarketDataProvider.Application.Validators;
using MarketDataProvider.Contracts.Requests;
using Moq.AutoMock;
using Xunit;

namespace MarketDataProvider.Application.UnitTests.Validators
{
    public class AggregateRequestValidatorUnitTests
    {
        private readonly AggregateRequestValidator _classUnderTest;
        private readonly Fixture _autofixture;
        private readonly AutoMocker _autoMocker;

        public AggregateRequestValidatorUnitTests()
        {
            _autoMocker = new AutoMocker();
            _autofixture = new Fixture();

            _classUnderTest = new AggregateRequestValidator();
        }

        [Fact]
        public void ValidRequest_Returns_Valid_Result()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            request.Ticker = "SPY";
            request.Multiplier = 1;
            request.From = DateTimeOffset.Now.AddDays(-1);
            request.To = DateTimeOffset.Now;

            // Act
            var response = _classUnderTest.Validate(request);

            // Assert
            response.IsValid.Should().BeTrue();
        }

        [Fact]
        public void InalidRequest_Returns_Error_Result()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            request.Ticker = "SPYYYYY";
            request.Multiplier = -1;
            request.From = DateTimeOffset.Now;
            request.To = DateTimeOffset.Now.AddDays(-1);

            // Act
            var response = _classUnderTest.Validate(request);

            // Assert
            response.IsValid.Should().BeFalse();
            response.Errors.Should().NotBeEmpty();
            response.Errors.Count().Should().Be(4);
        }
    }
}
