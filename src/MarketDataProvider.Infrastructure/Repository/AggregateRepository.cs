using MarketDataProvider.Clients.Interfaces;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using MarketDataProvider.Core.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Interfaces;
using Polygon.Client.Requests;
using Polygon.Client.Responses;
using Polygon.Client.Models;

namespace MarketDataProvider.Infrastructure.Repository
{
    public class AggregateRepository(
        IMapper mapper,
        IMarketCacheClient marketCacheClient,
        IPolygonClient polygonClient,
        IMemoryCache memoryCache,
        IAmazonS3 amazonS3Client,
        ILogger<AggregateRepository> logger) : IAggregateRepository
    {
        public async Task<AggregateResponse> QueryAggregateAsync(AggregateRequest request)
        {
            try
            {
                var cacheAggregateDto = await marketCacheClient.QueryAggregates<AggregateDto>(request);

                var days = DateUtilities.GetMarketOpenDays(request.From, request.To);

                if (cacheAggregateDto is not null && cacheAggregateDto.ToList().Count == days.Count)
                {
                    logger.LogInformation("Getting {ticker} aggregate from cache", request.Ticker);

                    var cacheAggregateResponse = mapper.Map<IEnumerable<AggregateDto>, AggregateResponse>(cacheAggregateDto);

                    return cacheAggregateResponse;
                }

                var polygonAggregateRequest = mapper.Map<AggregateRequest, PolygonAggregateRequest>(request);

                logger.LogInformation("Getting {ticker} aggregate from Polygon API", request.Ticker);

                var polygonAggregateResponse = await polygonClient.GetAggregates(polygonAggregateRequest);

                var aggregateDtos = mapper.Map<PolygonAggregateResponse, List<AggregateDto>>(polygonAggregateResponse);

                foreach (var aggregateDto in aggregateDtos)
                {
                    await marketCacheClient.SetAggregate("aggregate", aggregateDto, 120);
                }

                var aggregateResponse = mapper.Map<PolygonAggregateResponse, AggregateResponse>(polygonAggregateResponse);

                return aggregateResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<IEnumerable<string>> ScanAggregatesAsync(ScanPopulateRequest request)
        {
            try
            {
                var cacheResponse = await marketCacheClient.ScanAggregates<AggregateResponse>(request.Timestamp.Date);

                if (cacheResponse is not null && cacheResponse.Any())
                {
                    var cachedTickers = cacheResponse.Select(aggregate => aggregate.Ticker);
                    return cachedTickers;
                }

                var s3Request = new GetObjectRequest
                {
                    BucketName = "lad-dev-marketviewer",
                    Key = $"backtest/{request.Timestamp.Year}/{request.Timestamp.Month}/{request.Timestamp.Month}-{request.Timestamp.Day}.json"
                };

                var s3Response = await amazonS3Client.GetObjectAsync(s3Request);

                using var streamReader = new StreamReader(s3Response.ResponseStream);
                var json = await streamReader.ReadToEndAsync();

                var polygonResponses = JsonSerializer.Deserialize<IEnumerable<PolygonAggregateResponse>>(json);
                
                var tickers = polygonResponses.Select(q => q.Ticker);
                memoryCache.Set($"Tickers-{request.Timestamp:yyyy-MM-dd}", tickers);

                var aggregateDtos = mapper.Map<IEnumerable<PolygonAggregateResponse>, IEnumerable<AggregateDto>>(polygonResponses);

                var tasks = new List<Task>();
                foreach (var aggregateDto in aggregateDtos)
                {
                    tasks.Add(Task.Run(async () => await marketCacheClient.SetAggregate("scan", aggregateDto, expiresInMinutes: 15)));
                }
                await Task.WhenAll(tasks);

                return tickers;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<TickerDetails> GetTickerDetailsAsync(string ticker)
        {
            try
            {
                var polygonTickerDetailsResponse = await polygonClient.GetTickerDetails(ticker);

                if (polygonTickerDetailsResponse is null)
                {
                    logger.LogInformation("Invalid ticker.");
                    return null;
                }

                memoryCache.Set($"TickerDetails_{ticker}", polygonTickerDetailsResponse.TickerDetails);

                return polygonTickerDetailsResponse.TickerDetails;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
