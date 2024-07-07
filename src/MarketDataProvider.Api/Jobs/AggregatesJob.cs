using Amazon.S3.Model;
using Amazon.S3;
using MarketDataProvider.Contracts.Dtos;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using Quartz;
using System.Text.Json;
using AutoMapper;
using Polygon.Client.Requests;
using System.Diagnostics;
using MarketDataProvider.Clients.Interfaces;
using MarketDataProvider.Clients;

namespace MarketDataProvider.Api.Jobs
{
    public class AggregatesJob(
        IPolygonClient polygonClient,
        IMemoryCache memoryCache,
        IMapper mapper,
        IMarketCacheClient marketCacheClient,
        ILogger<AggregatesJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Populating aggregates at: {time}.", DateTimeOffset.Now);

            var sp = new Stopwatch();
            sp.Start();

            var tickers = memoryCache.Get<IEnumerable<string>>("Tickers");

            var tasks = new List<Task>();
            foreach (var ticker in tickers)
            {
                tasks.Add(Task.Run(() => PopulateAggregate(ticker)));
            }
            await Task.WhenAll(tasks);

            sp.Stop();

            logger.LogInformation("Finished populating aggregates at: {time}. Time elapsed: {elapsed}ms", DateTimeOffset.Now, sp.ElapsedMilliseconds);
        }

        private async Task PopulateAggregate(string ticker)
        {
            var polygonAggregateRequest = new PolygonAggregateRequest
            {
                Ticker = ticker,
                Multiplier = 1,
                Timespan = "minute",
                From = ((DateTimeOffset)DateTime.Now.Date).ToUnixTimeMilliseconds().ToString(),
                To = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
            };
            var polygonAggregateResponse = await polygonClient.GetAggregates(polygonAggregateRequest);

            var aggregateDto = mapper.Map<AggregateDto>(polygonAggregateResponse);

            memoryCache.Set($"Aggregate_{ticker}", aggregateDto);
            await marketCacheClient.SetAggregate("aggregate", aggregateDto, 1440);
        }
    }
}
