using System;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;

namespace HomeBudget.Rates.Api.MapperProfileConfigurations
{
    internal class CurrencyProfiler : Profile
    {
        public CurrencyProfiler()
            => CreateMap<NationalBankCurrency, Currency>()
                .ForMember(
                    dest => dest.DateEnd,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.DateEnd)))
                .ForMember(
                    dest => dest.DateStart,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.DateStart)));
    }
}
