using AutoFixture;
using AutoMapper;
using FluentAssertions;
using MarketDataProvider.Clients.Interfaces;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using MarketDataProvider.Infrastructure.Repository;
using Moq;
using Moq.AutoMock;
using Polygon.Client.Interfaces;
using Polygon.Client.Requests;
using Polygon.Client.Responses;
using Xunit;

namespace MarketDataProvider.Infrastructure.UnitTests.Repository
{
    public class AggregateRepositoryUnitTests
    {
        private readonly AggregateRepository _classUnderTest;
        private readonly Fixture _autofixture;
        private readonly AutoMocker _autoMocker;

        public AggregateRepositoryUnitTests()
        {
            _autoMocker = new AutoMocker();
            _autofixture = new Fixture();

            _classUnderTest = _autoMocker.CreateInstance<AggregateRepository>();
        }

        [Fact]
        public async Task MarketCache_Returns_Null_And_PolygonClient_Returns_Valid_Response()
        {
            // Arrange
            GivenAutomapperReturnsValidPolygonAggregateRequest();
            GivenPolygonClientReturnsValidResponse();
            GivenAutomapperReturnsValidAggregateDtoList();
            var mappedResponse = GivenAutomapperReturnsValidAggregateResponseFrom<PolygonAggregateResponse>();

            // Act
            var response = await _classUnderTest.QueryAggregateAsync(new AggregateRequest());

            // Assert
            response.Ticker.Should().Be(mappedResponse.Ticker);
            response.Results.Should().NotBeNullOrEmpty();
            response.Results.Count.Should().Be(mappedResponse.Results.Count);
        }

        [Fact]
        public async Task MarketCache_Adds_Proper_Number_Of_AggregateDtos()
        {
            // Arrange
            var aggregateDtos = GivenAutomapperReturnsValidAggregateDtoList();

            // Act
            await _classUnderTest.QueryAggregateAsync(new AggregateRequest());

            // Assert
            _autoMocker.GetMock<IMarketCacheClient>().Verify(method => method.SetAggregate(It.IsAny<string>(), It.IsAny<AggregateDto>(), It.IsAny<int>()), Times.Exactly(aggregateDtos.Count));
        }

        [Fact]
        public async Task MarketCache_Doesnt_Return_Enough_Aggregates()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            request.From = new DateTimeOffset(2024, 2, 26, 12, 0, 0, DateTimeOffset.Now.Offset);
            request.To = new DateTimeOffset(2024, 2, 29, 12, 0, 0, DateTimeOffset.Now.Offset);

            GivenMarketCacheReturnsAggregateDtos(count: 1);
            GivenAutomapperReturnsValidPolygonAggregateRequest();
            GivenPolygonClientReturnsValidResponse();
            var aggregateDtos = GivenAutomapperReturnsValidAggregateDtoList();
            GivenAutomapperReturnsValidAggregateResponseFrom<PolygonAggregateResponse>();

            // Act
            await _classUnderTest.QueryAggregateAsync(request);

            // Assert
            _autoMocker.Verify<IPolygonClient>(method => method.GetAggregates(It.IsAny<PolygonAggregateRequest>()), Times.Once());
            _autoMocker.GetMock<IMarketCacheClient>().Verify(method => method.SetAggregate(It.IsAny<string>(), It.IsAny<AggregateDto>(), It.IsAny<int>()), Times.Exactly(aggregateDtos.Count));
        }

        [Fact]
        public async Task MarketCache_Returns_All_Aggregates()
        {
            // Arrange
            var request = _autofixture.Create<AggregateRequest>();
            request.From = new DateTimeOffset(2024, 2, 26, 12, 0, 0, DateTimeOffset.Now.Offset);
            request.To = new DateTimeOffset(2024, 2, 29, 12, 0, 0, DateTimeOffset.Now.Offset);

            GivenMarketCacheReturnsAggregateDtos(count: 4);
            GivenAutomapperReturnsValidPolygonAggregateRequest();
            GivenPolygonClientReturnsValidResponse();
            GivenAutomapperReturnsValidAggregateDtoList();
            var mappedResponse = GivenAutomapperReturnsValidAggregateResponseFrom<IEnumerable<AggregateDto>>();

            // Act
            var response = await _classUnderTest.QueryAggregateAsync(request);

            // Assert
            response.Ticker.Should().Be(mappedResponse.Ticker);
            response.Results.Should().NotBeNullOrEmpty();
            response.Results.Count.Should().Be(mappedResponse.Results.Count);

            _autoMocker.GetMock<IMapper>().Verify(method => method.Map<AggregateRequest, PolygonAggregateRequest>(It.IsAny<AggregateRequest>()), Times.Never());
            _autoMocker.Verify<IPolygonClient>(method => method.GetAggregates(It.IsAny<PolygonAggregateRequest>()), Times.Never());
            _autoMocker.GetMock<IMapper>().Verify(method => method.Map<PolygonAggregateResponse, List<AggregateDto>>(It.IsAny<PolygonAggregateResponse>()), Times.Never());
            _autoMocker.GetMock<IMarketCacheClient>().Verify(method => method.SetAggregate(It.IsAny<string>(), It.IsAny<AggregateDto>(), default), Times.Never());
        }

        private void GivenAutomapperReturnsValidPolygonAggregateRequest()
        {
            _autoMocker.GetMock<IMapper>()
                .Setup(method => method.Map<AggregateRequest, PolygonAggregateRequest>(It.IsAny<AggregateRequest>()))
                .Returns(new PolygonAggregateRequest());
        }

        private List<AggregateDto> GivenAutomapperReturnsValidAggregateDtoList()
        {
            var response = _autofixture.Create<List<AggregateDto>>();
            _autoMocker.GetMock<IMapper>()
                .Setup(method => method.Map<PolygonAggregateResponse, List<AggregateDto>>(It.IsAny<PolygonAggregateResponse>()))
                .Returns(response);

            return response;
        }

        private AggregateResponse GivenAutomapperReturnsValidAggregateResponseFrom<TType>()
        {
            var response = _autofixture.Create<AggregateResponse>();
            _autoMocker.GetMock<IMapper>()
                .Setup(method => method.Map<TType, AggregateResponse>(It.IsAny<TType>()))
                .Returns(response);

            return response;
        }

        private void GivenPolygonClientReturnsValidResponse()
        {
            _autoMocker.GetMock<IPolygonClient>()
                .Setup(method => method.GetAggregates(It.IsAny<PolygonAggregateRequest>()))
                .ReturnsAsync(_autofixture.Create<PolygonAggregateResponse>());
        }

        private List<AggregateDto> GivenMarketCacheReturnsAggregateDtos(int count)
        {
            var response = new List<AggregateDto>();
            response.AddRange(_autofixture.CreateMany<AggregateDto>(count));

            _autoMocker.GetMock<IMarketCacheClient>()
                .Setup(method => method.QueryAggregates<AggregateDto>(It.IsAny<AggregateRequest>()))
                .ReturnsAsync(response);

            return response;
        }
    }
}
