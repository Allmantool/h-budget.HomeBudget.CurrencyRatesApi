using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Services
{
    internal class ConfigSettingsServices(
        ConfigSettings configSettings,
        IMapper mapper,
        INationalBankApiClient nationalBankApiClient,
        IConfigSettingsProvider configSettingsProvider)
        : IConfigSettingsServices
    {
        public async Task<Result<IReadOnlyCollection<Currency>>> GetAvailableCurrenciesAsync()
        {
            var currencies = await nationalBankApiClient.GetCurrenciesAsync();
            var upToDateCurrencies = currencies.Where(c => c.DateStart <= DateTime.Today && c.DateEnd >= DateTime.Today);

            return Result.Succeeded(mapper.Map<IReadOnlyCollection<Currency>>(upToDateCurrencies));
        }

        public async Task<Result<int>> SaveSettingsAsync(ConfigSettings settings)
        {
            return Result.Succeeded(await configSettingsProvider.SaveDefaultSettingsAsync(settings));
        }

        public Task<Result<ConfigSettings>> GetSettingsAsync()
        {
            return Task.FromResult(Result.Succeeded(configSettings));
        }
    }
}
