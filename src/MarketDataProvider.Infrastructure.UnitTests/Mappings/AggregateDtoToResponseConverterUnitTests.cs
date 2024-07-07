using AutoFixture;
using FluentAssertions;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Models;
using MarketDataProvider.Infrastructure.Mappings;
using Polygon.Client.Models;
using Xunit;

namespace MarketDataProvider.Infrastructure.UnitTests.Mappings
{
    public class AggregateDtoToResponseConverterUnitTests
    {
        private readonly AggregateDtoToResponseConverter _classUnderTest;
        private readonly IFixture _autoFixture;

        public AggregateDtoToResponseConverterUnitTests()
        {
            _autoFixture = new Fixture();

            _classUnderTest = new AggregateDtoToResponseConverter();
        }

        [Fact]
        public void AggregateDtoList_Converts_To_Valid_AggregateResponse()
        {
            // Arrange
            var aggregateDtos = _autoFixture.CreateMany<AggregateDto>(3).ToArray();
            aggregateDtos[0].Ticker = "SPY";
            aggregateDtos[1].Ticker = "SPY";
            aggregateDtos[2].Ticker = "SPY";

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Assert
            response.Ticker.Should().Be(aggregateDtos.First().Ticker);
            response.Results.Count.Should().Be(GetResultsCount(aggregateDtos));
        }

        [Fact]
        public void AggregateDtoList_With_Null_Results_Converts_To_Valid_AggregateResponse()
        {
            // Arrange
            var aggregateDtos = _autoFixture.CreateMany<AggregateDto>(3).ToArray();
            aggregateDtos[0].Ticker = "SPY";
            aggregateDtos[1].Ticker = "SPY";
            aggregateDtos[2].Ticker = "SPY";
            aggregateDtos[1].Results = null;

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            //// Assert
            response.Ticker.Should().Be(aggregateDtos.First().Ticker);
            response.Results.Count.Should().Be(GetResultsCount(aggregateDtos));
        }

        [Fact]
        public void AggregateDtoList_With_Mixed_Tickers_Returns_Null()
        {
            // Arrange
            var aggregateDtos = _autoFixture.CreateMany<AggregateDto>(3).ToArray();
            aggregateDtos[0].Ticker = "SPY";
            aggregateDtos[1].Ticker = "META";
            aggregateDtos[2].Ticker = "SPY";

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Act
            response.Should().BeNull();
        }

        [Fact]
        public void AggregateDtoList_With_Null_Ticker_Returns_Null()
        {
            // Arrange
            var aggregateDtos = _autoFixture.CreateMany<AggregateDto>(3).ToArray();
            aggregateDtos[0].Ticker = "SPY";
            aggregateDtos[1].Ticker = "SPY";
            aggregateDtos[2].Ticker = null;

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Act
            response.Should().BeNull();
        }

        [Fact]
        public void Null_AggregateDtoList_Returns_Null()
        {
            // Arrange
            var aggregateDtos = _autoFixture.CreateMany<AggregateDto>(3).ToArray();
            aggregateDtos[0].Results = null;
            aggregateDtos[1].Results = null;
            aggregateDtos[2].Results = null;
            
            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Act
            response.Should().BeNull();
        }

        [Fact]
        public void Empty_AggregateDtoList_Returns_Null()
        {
            // Arrange
            var aggregateDtos = _autoFixture.CreateMany<AggregateDto>(3).ToArray();
            aggregateDtos[0].Results = Enumerable.Empty<Bar>().ToList();
            aggregateDtos[1].Results = Enumerable.Empty<Bar>().ToList();
            aggregateDtos[2].Results = Enumerable.Empty<Bar>().ToList();

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Act
            response.Should().BeNull();
        }

        [Fact]
        public void Null_Source_Returns_Null()
        {
            // Act
            var response = _classUnderTest.Convert(null, null, null);

            // Act
            response.Should().BeNull();
        }

        private static int GetResultsCount(IEnumerable<AggregateDto> aggregates)
        {
            var count = 0;

            foreach (var aggregate in aggregates)
            {
                if (aggregate.Results != null)
                {
                    count += aggregate.Results.Count;
                }
            }

            return count;
        }
    }
}
