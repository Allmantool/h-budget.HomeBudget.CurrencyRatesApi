using AutoMapper;

using Microsoft.Extensions.Logging.Abstractions;

using HomeBudget.Rates.Api.MapperProfileConfigurations;

namespace HomeBudget.Rates.Api.Tests.Mappers
{
    [TestFixture]
    public class ApiRatesMappingProfilesTests
    {
        private MapperConfiguration _mapperConfiguration;

        [SetUp]
        public void Setup()
        {
            var configurationExpression = new MapperConfigurationExpression();
            configurationExpression.AddMaps(ApiRatesMappingProfiles.GetExecutingAssembly());

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