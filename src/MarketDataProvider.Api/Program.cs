using HealthChecks.UI.Client;
using MarketDataProvider.Api.BackgroundServices;
using MarketDataProvider.Api.Healthchecks;
using MarketDataProvider.Api.Jobs;
using MarketDataProvider.Application;
using MarketDataProvider.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Quartz;
using System.Diagnostics.CodeAnalysis;

namespace MarketDataProvider.Api
{
    public class Program
    {
        [ExcludeFromCodeCoverage]
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            DotNetEnv.Env.Load($"{builder.Environment.EnvironmentName}.env");
            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddQuartz();
            builder.Services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });

            builder.Services.RegisterApplication()
                .RegisterInfrastructure(builder.Configuration);

            builder.Services.AddHttpClient()
                .AddMemoryCache()
                .AddHostedService<PopulateCache>()
                .AddHealthChecks()
                .AddCheck<HealthCheck>("HealthCheck");

            //var now = DateTimeOffset.Now;
            //var startTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 0, 1, now.Offset);

            //var aggregatesJob = JobBuilder.Create<AggregatesJob>()
            //    .WithIdentity("aggregates", "group1")
            //    .Build(); 
            
            //var snapshotJob = JobBuilder.Create<SnapshotJob>()
            //    .WithIdentity("snapshot", "group2")
            //    .Build();

            //var aggregatesTrigger = TriggerBuilder.Create()
            //    .WithIdentity("trigger1", "group1")
            //    .StartAt(startTime)
            //    .ForJob(aggregatesJob)
            //    .Build(); 
            
            //var snapshotTrigger = TriggerBuilder.Create()
            //    .WithIdentity("trigger2", "group2")
            //    .StartAt(startTime.AddMinutes(1))
            //    .WithSimpleSchedule(schedule => schedule
            //        .WithIntervalInMinutes(1)
            //        .RepeatForever())
            //    .ForJob(snapshotJob)
            //    .Build();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //ThreadPool.SetMinThreads(1000, 1000);

            //var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
            //var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

            //scheduler.ScheduleJob(aggregatesJob, aggregatesTrigger).GetAwaiter().GetResult();
            //scheduler.ScheduleJob(snapshotJob, snapshotTrigger).GetAwaiter().GetResult();

            // Configure the HTTP request pipeline.
            if (IsDevelopment(app.Environment.EnvironmentName))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            // app.MapHealthChecks("/health", new HealthCheckOptions
            // {
            //     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            // });
        }

        private static bool IsDevelopment(string environment)
        {
            return environment switch
            {
                "docker" => true,
                "local" => true,
                "dev" => true,
                "qa" => false,
                "cert" => false,
                "prod" => false,
                _ => false
            };
        }
    }
}