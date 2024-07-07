using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarketDataProvider.Api.Healthchecks
{
    public class HealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Healthy"));
        }
    }
}
