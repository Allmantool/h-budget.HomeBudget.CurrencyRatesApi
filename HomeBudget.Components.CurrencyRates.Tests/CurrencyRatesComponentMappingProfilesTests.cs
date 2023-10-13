using AutoMapper;
using NUnit.Framework;

using HomeBudget.Rates.Api.MapperProfileConfigurations;

namespace HomeBudget.Components.CurrencyRates.Tests
{
    [TestFixture]
    public class CurrencyRatesComponentMappingProfilesTests
    {
        private readonly MapperConfiguration _mapperConfiguration = new (x => x.AddMaps(ApiCallMappingProfiles.GetExecutingAssembly()));

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