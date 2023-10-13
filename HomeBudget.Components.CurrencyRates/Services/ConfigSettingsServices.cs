using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Models;
using HomeBudget.Core.Services;

namespace HomeBudget.Components.CurrencyRates.Services
{
    internal class ConfigSettingsServices : BaseService, IConfigSettingsServices
    {
        private readonly IMapper _mapper;
        private readonly INationalBankApiClient _nationalBankApiClient;
        private readonly IConfigSettingsProvider _configSettingsProvider;

        private readonly ConfigSettings _configSettings;

        public ConfigSettingsServices(
            ConfigSettings configSettings,
            IMapper mapper,
            INationalBankApiClient nationalBankApiClient,
            IConfigSettingsProvider configSettingsProvider)
        {
            _mapper = mapper;
            _nationalBankApiClient = nationalBankApiClient;
            _configSettingsProvider = configSettingsProvider;
            _configSettings = configSettings;
        }

        public async Task<Result<IReadOnlyCollection<Currency>>> GetAvailableCurrenciesAsync()
        {
            var currencies = await _nationalBankApiClient.GetCurrenciesAsync();
            var upToDateCurrencies = currencies.Where(c => c.DateStart <= DateTime.Today && c.DateEnd >= DateTime.Today);

            return Succeeded(_mapper.Map<IReadOnlyCollection<Currency>>(upToDateCurrencies));
        }

        public async Task<Result<int>> SaveSettingsAsync(ConfigSettings settings)
        {
            return Succeeded(await _configSettingsProvider.SaveDefaultSettingsAsync(settings));
        }

        public async Task<Result<ConfigSettings>> GetSettingsAsync()
        {
            return Succeeded(await Task.FromResult(_configSettings));
        }
    }
}
