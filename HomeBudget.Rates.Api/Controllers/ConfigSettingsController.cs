﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Rates.Api.Controllers
{
    [ApiController]
    [Route(Endpoints.ConfigurationSettingsApi, Name = Endpoints.ConfigurationSettingsApi)]
    public class ConfigSettingsController(IConfigSettingsServices configSettingsServices) : ControllerBase
    {
        [HttpGet]
        public async Task<Result<ConfigSettings>> GetSettingsAsync()
        {
            return await configSettingsServices.GetSettingsAsync();
        }

        [HttpGet("/currencies")]
        public async Task<Result<IReadOnlyCollection<Currency>>> GetAvailableCurrenciesAsync()
        {
            return await configSettingsServices.GetAvailableCurrenciesAsync();
        }

        [HttpPost]
        public async Task<Result<int>> SaveSettingsAsync(ConfigSettings settings)
        {
            return await configSettingsServices.SaveSettingsAsync(settings);
        }
    }
}
