using AutoFixture;
using FluentAssertions;
using MarketDataProvider.Infrastructure.Mappings;
using Polygon.Client.Models;
using Polygon.Client.Responses;
using System.Text;
using Xunit;

namespace MarketDataProvider.Infrastructure.UnitTests.Mappings
{
    public class PolygonAggregateToDtoConverterUnitTests
    {
        private readonly PolygonAggregateToDtoConverter _classUnderTest;
        private readonly IFixture _autoFixture;

        public PolygonAggregateToDtoConverterUnitTests()
        {
            _autoFixture = new Fixture();

            _classUnderTest = new PolygonAggregateToDtoConverter();
        }

        [Fact]
        public void PolygonAggregate_Converts_To_Valid_AggregateDtoList()
        {
            // Arrange
            var aggregateDtos = _autoFixture.Create<PolygonAggregateResponse>();

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Assert
            foreach (var item in response)
            {
                item.Ticker.Should().Be(aggregateDtos.Ticker);
                item.Results.Count.Should().Be(aggregateDtos.Results.Count());
                item.Count.Should().Be(aggregateDtos.Results.Count());
            }
        }

        [Fact]
        public void PolygonAggregate_With_Single_Candle_Converts_To_Valid_AggregateDtoList()
        {
            // Arrange
            var aggregateDtos = _autoFixture.Create<PolygonAggregateResponse>();
            aggregateDtos.Results = new List<Bar>
            {
                _autoFixture.Create<Bar>()
            };

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Assert
            foreach (var item in response)
            {
                item.Ticker.Should().Be(aggregateDtos.Ticker);
                item.Results.Count.Should().Be(aggregateDtos.Results.Count());
                item.Count.Should().Be(aggregateDtos.Results.Count());
            }
        }

        [Fact]
        public void Null_PolygonAggregate_Returns_EmptyList()
        {
            // Act
            var response = _classUnderTest.Convert(null, null, null);

            // Assert
            response.Should().NotBeNull();
            response.Should().BeEmpty();
        }

        [Fact]
        public void PolygonAggregate_With_Null_Results_Returns_EmptyList()
        {
            // Arrange
            var aggregateDtos = _autoFixture.Create<PolygonAggregateResponse>();
            aggregateDtos.Results = null;
            
            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Assert
            response.Should().NotBeNull();
            response.Should().BeEmpty();
        }

        [Fact]
        public void PolygonAggregate_With_Empty_Results_Returns_EmptyList()
        {
            // Arrange
            var aggregateDtos = _autoFixture.Create<PolygonAggregateResponse>();
            aggregateDtos.Results = Enumerable.Empty<Bar>();

            // Act
            var response = _classUnderTest.Convert(aggregateDtos, null, null);

            // Assert
            response.Should().NotBeNull();
            response.Should().BeEmpty();
        }
    }
}
