using AutoMapper;

using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.MapperProfileConfigurations;

namespace HomeBudget.Components.CurrencyRates.Tests.Mappers
{
    [TestFixture]
    public class CurrencyRatesMappingProfilesTests
    {
        private MapperConfiguration _mapperConfiguration;

        [SetUp]
        public void Setup()
        {
            var configurationExpression = new MapperConfigurationExpression();
            configurationExpression.AddMaps(CurrencyRatesMappingProfiles.GetExecutingAssembly());

            _mapperConfiguration = new MapperConfiguration(configurationExpression, NullLoggerFactory.Instance);

            _mapperConfiguration.CreateMapper();
        }

        [Test]
        public void AssertConfigurationIsValid_WithValidMappingConfiguration_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _mapperConfiguration.AssertConfigurationIsValid());
        }
    }
}