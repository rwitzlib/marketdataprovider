using MarketDataProvider.Clients.Interfaces;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Requests;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using Polygon.Client.Models;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace MarketDataProvider.Clients
{
    public class MarketCacheClient(
        IConnectionMultiplexer connectionMultiplexer,
        IMemoryCache memoryCache,
        ILogger<MarketCacheClient> logger) : IMarketCacheClient
    {
        public async Task<IEnumerable<TAggregateType>> QueryAggregates<TAggregateType>(AggregateRequest request)
        {
            try
            {
                if (request is null
                || request.Ticker is null
                || request.Multiplier is 0
                || request.From > request.To)
                {
                    return [];
                }

                var database = connectionMultiplexer.GetDatabase();
                var ft = database.FT();

                var query = $"{request.Ticker} @Date:[{request.From:yyyyMMdd} {request.To:yyyyMMdd}]";

                logger.LogInformation("Query for {aggregate}: {query}", request.Ticker, query);

                var queryResponse = await ft.SearchAsync("idx:aggregates", new Query(query).Limit(0, 10000));

                var aggregateDtos = queryResponse.Documents.Select(document => JsonSerializer.Deserialize<TAggregateType>(document["json"]));

                return aggregateDtos;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return [];
            }
        }

        public async Task<IEnumerable<TAggregateType>> ScanAggregates<TAggregateType>(DateTime date)
        {
            try
            {
                if (date > DateTime.Now)
                {
                    return [];
                }

                var database = connectionMultiplexer.GetDatabase();
                var ft = database.FT();
                var tickers = memoryCache.Get<IEnumerable<string>>($"Tickers-{date:yyyy-MM-dd}") ?? [];

                var tasks = new List<Task<TAggregateType>>();
                foreach (var ticker in tickers)
                {
                    tasks.Add(Task.Run(async () => await RunScanQuery<TAggregateType>(ticker, date)));
                }
                var results = await Task.WhenAll(tasks);

                var aggregates = results.Where(q => q is not null);

                return aggregates;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return [];
            }
        }

        public async Task<TickerDetails> GetTickerDetails(string ticker)
        {
            try
            {
                if (ticker is null)
                {
                    return null;
                }

                var database = connectionMultiplexer.GetDatabase();
                var ft = database.FT();
                var query = $"{ticker}";
                var queryResponse = await ft.SearchAsync("idx:tickerdetails", new Query(query));

                var tickerDetails = JsonSerializer.Deserialize<TickerDetails>(queryResponse.Documents.Select(document => document["json"]).FirstOrDefault());

                return tickerDetails;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }
        
        public async Task SetAggregate(string prefix, AggregateDto aggregate, int expiresInMinutes = -1)
        {
            try
            {
                if (aggregate is null
                || aggregate.Ticker is null
                || aggregate.Results is null
                || aggregate.Results.Count == 0)
                {
                    return;
                }

                var database = connectionMultiplexer.GetDatabase();
                var json = database.JSON();

                var key = $"{prefix}:{aggregate.Ticker}_{aggregate.Date}";
                await json.SetAsync(key, "$", aggregate);

                if (expiresInMinutes != -1)
                {
                    await database.KeyExpireAsync(key, TimeSpan.FromMinutes(expiresInMinutes));
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error setting aggregate for {aggregate.Ticker}: {ex.Message}");
            }
        }

        public async Task<TickerDetails> SetTickerDetails(string prefix, TickerDetails tickerDetails, int expiresInMinutes = -1)
        {
            try
            {
                if (tickerDetails is null
                    || tickerDetails.Ticker is null)
                {
                    return null;
                }

                var database = connectionMultiplexer.GetDatabase();
                var json = database.JSON();

                var key = $"{prefix}:{tickerDetails.Ticker}";
                var response = await json.SetAsync(key, "$", tickerDetails);

                if (expiresInMinutes != -1)
                {
                    var expireResponse = await database.KeyExpireAsync(key, TimeSpan.FromMinutes(expiresInMinutes));
                }

                if (response)
                {
                    return tickerDetails;
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error setting ticker details for {tickerDetails.Ticker}: {ex.Message}");
                return null;
            }
        }

        private async Task<TAggregateType> RunScanQuery<TAggregateType>(string ticker, DateTime date)
        {
            try
            {
                var database = connectionMultiplexer.GetDatabase();
                var ft = database.FT();

                var query = $"{ticker} @Date:[{date:yyyyMMdd} {date:yyyyMMdd}]";
                var queryResponse = await ft.SearchAsync("idx:scans", new Query(query).Limit(0, 1));

                var aggregateDtos = queryResponse.Documents.Select(document => JsonSerializer.Deserialize<TAggregateType>(document["json"])).FirstOrDefault();

                return aggregateDtos;
            }
            catch (Exception e)
            {
                return default;
            }
        }
    }
}

