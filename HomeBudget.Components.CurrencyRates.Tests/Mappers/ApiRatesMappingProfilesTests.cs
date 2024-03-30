using AutoMapper;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.MapperProfileConfigurations;

namespace HomeBudget.Components.CurrencyRates.Tests.Mappers
{
    [TestFixture]
    public class CurrencyRatesMappingProfilesTests
    {
        private readonly MapperConfiguration _mapperConfiguration = new(x => x.AddMaps(CurrencyRatesMappingProfiles.GetExecutingAssembly()));

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