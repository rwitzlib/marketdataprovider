using AutoMapper;
using Xunit;
using MarketDataProvider.Infrastructure.Mappings;

namespace MarketDataProvider.Infrastructure.UnitTests.Mappings
{
    public class AggregateProfileUnitTests
    {
        [Fact]
        public void AggregateProfile_Is_Valid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AggregateProfile>());

            config.AssertConfigurationIsValid();
        }
    }
}
