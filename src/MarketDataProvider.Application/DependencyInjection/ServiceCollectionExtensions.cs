using FluentValidation;
using MarketDataProvider.Application.Handlers;
using MarketDataProvider.Application.Validators;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Infrastructure.Mappings;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MarketDataProvider.Application;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
    {
        var microserviceApplicationAssemblies = new[]
        {
            typeof(AggregateHandler).Assembly,
            typeof(AggregateProfile).Assembly
        };

        services.AddMediatR(services => services.RegisterServicesFromAssemblies(microserviceApplicationAssemblies))
            .AddAutoMapper(microserviceApplicationAssemblies)
            .AddScoped<IValidator<AggregateRequest>, AggregateRequestValidator>()
            .AddScoped<IValidator<string>, TickerDetailsValidator>()
            .AddScoped<IValidator<ScanPopulateRequest>, ScanValidator>();

        return services;
    }
}
