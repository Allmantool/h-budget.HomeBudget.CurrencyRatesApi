using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Models;

namespace HomeBudget.Rates.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigSettingsController : ControllerBase
    {
        private readonly IConfigSettingsServices _configSettingsServices;

        public ConfigSettingsController(IConfigSettingsServices configSettingsServices)
        {
            _configSettingsServices = configSettingsServices;
        }

        [HttpGet]
        public async Task<Result<ConfigSettings>> GetSettingsAsync()
        {
            return await _configSettingsServices.GetSettingsAsync();
        }

        [HttpGet("/configSettings/availableCurrencies")]
        public async Task<Result<IReadOnlyCollection<Currency>>> GetAvailableCurrencies()
        {
            return await _configSettingsServices.GetAvailableCurrenciesAsync();
        }

        [HttpPost]
        public async Task<Result<int>> SaveSettingsAsync(ConfigSettings settings)
        {
            return await _configSettingsServices.SaveSettingsAsync(settings);
        }
    }
}
