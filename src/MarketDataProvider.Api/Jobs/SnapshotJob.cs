using MarketDataProvider.Clients.Interfaces;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Requests;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using Quartz;
using System.Diagnostics;

namespace MarketDataProvider.Api.Jobs
{
    public class SnapshotJob(
        IPolygonClient polygonClient,
        IMemoryCache memoryCache,
        IMarketCacheClient marketCacheClient,
        ILogger<SnapshotJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Snapshot at: {time}.", DateTimeOffset.Now);

            var sp = new Stopwatch();
            sp.Start();

            var snapshotResponse = await polygonClient.GetAllTickersSnapshot(null);

            var tasks = new List<Task>();
            foreach (var snapshot in snapshotResponse.Tickers)
            {
                tasks.Add(Task.Run(() => AddBarToCache(snapshot.Ticker, snapshot.Minute)));
            }
            await Task.WhenAll(tasks);

            sp.Stop();

            logger.LogInformation("Finished snapshot at: {time}. Time elapsed: {elapsed}ms", DateTimeOffset.Now, sp.ElapsedMilliseconds);
        }

        private async Task AddBarToCache(string ticker, Bar bar)
        {
            var aggregate = memoryCache.Get<AggregateDto>($"Aggregate_{ticker}");

            aggregate?.Results.Add(bar);

            memoryCache.Set($"Aggregate_{ticker}", aggregate);
            await marketCacheClient.SetAggregate("aggregate", aggregate, 1440);
        }
    }
}
