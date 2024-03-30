using AutoMapper;

using HomeBudget.Rates.Api.MapperProfileConfigurations;

namespace HomeBudget.Rates.Api.Tests.Mappers
{
    [TestFixture]
    public class ApiRatesMappingProfilesTests
    {
        private readonly MapperConfiguration _mapperConfiguration = new(x => x.AddMaps(ApiRatesMappingProfiles.GetExecutingAssembly()));

        [SetUp]
        public void Setup()
        {
            _mapperConfiguration.CreateMapper();
        }

        [Test]
        public void AssertConfigurationIsValid_WithValidMappingConfiguration_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _mapperConfiguration.AssertConfigurationIsValid());
        }
    }
}