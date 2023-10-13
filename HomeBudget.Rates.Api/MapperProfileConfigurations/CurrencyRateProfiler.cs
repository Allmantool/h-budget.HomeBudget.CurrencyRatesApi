using AutoMapper;
using CurrencyRate = HomeBudget.Rates.Api.Models.CurrencyRate;

namespace HomeBudget.Rates.Api.MapperProfileConfigurations
{
    internal class CurrencyRateProfiler : Profile
    {
        public CurrencyRateProfiler()
            => CreateMap<CurrencyRate, HomeBudget.Components.CurrencyRates.Models.CurrencyRate>()
                .ForMember(
                    dest => dest.CurrencyId,
                    opt => opt.MapFrom(src => src.CurrencyId));
    }
}
