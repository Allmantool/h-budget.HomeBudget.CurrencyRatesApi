using AutoMapper;

using HomeBudget.Components.Exchange.Models;
using HomeBudget.Rates.Api.Models.Requests;

namespace HomeBudget.Rates.Api.MapperProfileConfigurations
{
    internal class ExchangeMultiplierProfiler : Profile
    {
        public ExchangeMultiplierProfiler()
        {
            CreateMap<CurrencyExchangeMultiplierRequest, ExchangeMultiplierQuery>();

            CreateMap<CurrencyExchangeRequest, ExchangeMultiplierQuery>();
        }
    }
}
