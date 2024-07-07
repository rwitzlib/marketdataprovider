using MarketDataProvider.Clients.Interfaces;
using MarketDataProvider.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using MarketDataProvider.Core.Interfaces;
using MarketDataProvider.Infrastructure.Repository;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using Amazon.S3;
using Amazon;
using Polygon.Client.DependencyInjection;
using MarketDataProvider.Clients.Configuration;

namespace MarketDataProvider.Infrastructure.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var polygonToken = configuration.GetSection("Tokens").GetValue<string>("PolygonApi");
            var marketCacheConfiguration = configuration.GetSection("Clients").GetSection("MarketCache").Get<MarketCacheConfiguration>();

            services.AddSingleton<IAggregateRepository, AggregateRepository>()
                .AddSingleton<IAmazonS3>(client => new AmazonS3Client(RegionEndpoint.USEast2))
                .AddSingleton<IMarketCacheClient, MarketCacheClient>()
                .AddPolygonClient(polygonToken)
                .AddHostedService<AggregatesLiveFeed>();

            var connectionMultiplexer = ConnectionMultiplexer.Connect(marketCacheConfiguration.Url);

            var aggregateIndexCreated = CreateAggregatesIndex(connectionMultiplexer);
            var scanIndexCreated = CreateScanIndex(connectionMultiplexer);
            var tickerIndexCreated = CreateTickerDetailsIndex(connectionMultiplexer);

            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

            return services;
        }

        private static bool CreateAggregatesIndex(ConnectionMultiplexer connectionMultiplexer)
        {
            try
            {
                var database = connectionMultiplexer.GetDatabase();
                var ft = database.FT();

                var schema = new Schema()
                    .AddTextField(new FieldName("$.Ticker", "Ticker"))
                    .AddNumericField(new FieldName("$.Date", "Date"));

                var success = ft.Create("idx:aggregates", new FTCreateParams().On(IndexDataType.JSON).Prefix("aggregate:"), schema);

                return success;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static bool CreateScanIndex(ConnectionMultiplexer connectionMultiplexer)
        {
            try
            {
                var database = connectionMultiplexer.GetDatabase();
                var ft = database.FT();

                var schema = new Schema()
                    .AddTextField(new FieldName("$.Ticker", "Ticker"))
                    .AddNumericField(new FieldName("$.Date", "Date"));

                var success = ft.Create("idx:scans", new FTCreateParams().On(IndexDataType.JSON).Prefix("scan:"), schema);

                return success;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static bool CreateTickerDetailsIndex(ConnectionMultiplexer connectionMultiplexer)
        {
            try
            {
                var database = connectionMultiplexer.GetDatabase();
                var ft = database.FT();

                var schema = new Schema()
                    .AddTextField(new FieldName("$.ticker", "ticker"));

                var success = ft.Create("idx:tickerdetails", new FTCreateParams().On(IndexDataType.JSON).Prefix("ticker:"), schema);

                return success;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
