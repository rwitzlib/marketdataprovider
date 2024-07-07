using AutoFixture;
using MarketDataProvider.Clients.Interfaces;
using Moq.AutoMock;
using Moq;
using StackExchange.Redis;
using Xunit;
using MarketDataProvider.Contracts.Requests;
using FluentAssertions;
using MarketDataProvider.Contracts.Dtos;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Models;

namespace MarketDataProvider.Clients.UnitTests
{
    public class MarketCacheClientUnitTests
    {
        private readonly Fixture _autoFixture;
        private readonly AutoMocker _autoMocker;
        private readonly IMarketCacheClient _classUnderTest;

        public MarketCacheClientUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();

            _classUnderTest = new MarketCacheClient(new Mock<IConnectionMultiplexer>().Object, new Mock<IMemoryCache>().Object, new NullLogger<MarketCacheClient>());
        }

        [Fact]
        public async Task GetAggregate_Null_Request_Returns_EmptyResponse()
        {
            // Act
            var response = await _classUnderTest.QueryAggregates<AggregateDto>(null);

            // Assert
            response.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetAggregate_Null_Ticker_Returns_EmptyResponse()
        {
            // Arrange
            var request = _autoFixture.Create<AggregateRequest>();
            request.Ticker = null;

            // Act
            var response = await _classUnderTest.QueryAggregates<AggregateDto>(request);

            // Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAggregate_Invalid_Multiplier_Returns_EmptyResponse()
        {
            // Arrange
            var request = _autoFixture.Create<AggregateRequest>();
            request.Multiplier = 0;

            // Act
            var response = await _classUnderTest.QueryAggregates<AggregateDto>(request);

            // Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAggregate_Invalid_Dates_Returns_EmptyResponse()
        {
            // Arrange
            var request = _autoFixture.Create<AggregateRequest>();
            request.From = DateTime.UtcNow;
            request.To = DateTime.UtcNow.AddDays(-1);

            // Act
            var response = await _classUnderTest.QueryAggregates<AggregateDto>(request);

            // Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public async Task SetAggregate_Null_Aggregate_Returns_Null()
        {
            // Act
            await _classUnderTest.SetAggregate("aggregate", null);

            // Assert
            //response.Should().BeNull();
        }
        
        [Fact]
        public async Task SetAggregate_Invalid_Ticker_Returns_Null()
        {
            // Arrange
            var aggregate = _autoFixture.Create<AggregateDto>();
            aggregate.Ticker = null;

            // Act
            await _classUnderTest.SetAggregate("aggregate", aggregate);

            // Assert
            //response.Should().BeNull();
        }

        [Fact]
        public async Task SetAggregate_Null_Results_Returns_Null()
        {
            // Arrange
            var aggregate = _autoFixture.Create<AggregateDto>();
            aggregate.Results = null;

            // Act
            await _classUnderTest.SetAggregate("aggregate", aggregate);

            // Assert
            //response.Should().BeNull();
        }

        [Fact]
        public async Task SetAggregate_Empty_Results_Returns_Null()
        {
            // Arrange
            var aggregate = _autoFixture.Create<AggregateDto>();
            aggregate.Results = new List<Bar>();

            // Act
            await _classUnderTest.SetAggregate("aggregate", aggregate);

            // Assert
            //response.Should().BeNull();
        }
    }
}
