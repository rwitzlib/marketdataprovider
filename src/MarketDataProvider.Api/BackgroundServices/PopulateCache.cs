using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Models;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace MarketDataProvider.Api.BackgroundServices
{
    [ExcludeFromCodeCoverage]
    public class PopulateCache(
        IAmazonS3 amazonS3Client,
        IMemoryCache memoryCache,
        ILogger<PopulateCache> logger) : IHostedLifecycleService
    {
        public async Task StartingAsync(CancellationToken cancellationToken)
        {
           
            logger.LogInformation("StartingAsync");

            await PopulateTickerDetails();
        }

        private async Task PopulateTickerDetails()
        {
            logger.LogInformation("Populating ticker details at {time}", DateTimeOffset.Now);
            var sp = new Stopwatch();
            sp.Start();
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = "lad-dev-marketviewer",
                    Key = "tickerdetails/stocks.json"
                };
                var s3Response = await amazonS3Client.GetObjectAsync(request);

                using var streamReader = new StreamReader(s3Response.ResponseStream);
                var json = await streamReader.ReadToEndAsync();

                var tickerDetails = JsonSerializer.Deserialize<IEnumerable<TickerDetails>>(json);

                var tickers = tickerDetails.Select(tickerDetails => tickerDetails.Ticker);
                memoryCache.Set("Tickers", tickers);

                foreach (var tickerDetail in tickerDetails)
                {
                    memoryCache.Set($"TickerDetails_{tickerDetail.Ticker}", tickerDetail);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error getting ticker details: {message}", ex.Message);
            }
            sp.Stop();
            logger.LogInformation("Ticker details populated at {time}. Time elapsed: {elapsed}ms", DateTimeOffset.Now, sp.ElapsedMilliseconds);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StartAsync");
            return Task.CompletedTask;
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StartedAsync");
            return Task.CompletedTask;
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StoppingAsync");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StopAsync");
            return Task.CompletedTask;
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("StoppedAsync");
            return Task.CompletedTask;
        }
    }
}
