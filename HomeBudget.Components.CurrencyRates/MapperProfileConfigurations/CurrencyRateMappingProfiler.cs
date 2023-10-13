using System;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;

namespace HomeBudget.Components.CurrencyRates.MapperProfileConfigurations
{
    internal class CurrencyRateMappingProfiler : Profile
    {
        private const int RatesPrecision = 5;

        public CurrencyRateMappingProfiler()
        {
            CreateMap<NationalBankCurrencyRate, CurrencyRate>()
                .ForMember(
                    dest => dest.UpdateDate,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.UpdateDate)))
                .ForMember(
                    dest => dest.RatePerUnit,
                    opt => opt.MapFrom(src => Math.Round((decimal)src.OfficialRate / src.Scale, RatesPrecision)));

            CreateMap<CurrencyRate, CurrencyRateEntity>()
                .ForMember(
                    dest => dest.UpdateDate,
                    opt => opt.MapFrom(src => src.UpdateDate.ToDateTime(TimeOnly.MinValue)));

            CreateMap<CurrencyRateEntity, CurrencyRate>()
                .ForMember(
                    dest => dest.UpdateDate,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.UpdateDate)));

            CreateMap<NationalBankShortCurrencyRate, CurrencyRate>()
                .ForMember(
                    dest => dest.UpdateDate,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.UpdateDate)));
        }
    }
}
