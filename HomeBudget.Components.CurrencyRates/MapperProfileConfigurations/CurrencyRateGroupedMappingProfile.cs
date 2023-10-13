using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HomeBudget.Components.CurrencyRates.Models;

namespace HomeBudget.Components.CurrencyRates.MapperProfileConfigurations
{
    internal class CurrencyRateGroupedMappingProfile : Profile
    {
        public CurrencyRateGroupedMappingProfile()
        {
            CreateMap<IEnumerable<CurrencyRate>, CurrencyRateGrouped>()
                .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.FirstOrDefault().CurrencyId))
                .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom(src => src.FirstOrDefault().Abbreviation))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstOrDefault().Name))
                .ForMember(dest => dest.Scale, opt => opt.MapFrom(src => src.FirstOrDefault().Scale))
                .ForMember(dest => dest.RateValues, opt => opt.MapFrom(src => src));

            CreateMap<CurrencyRate, CurrencyRateValue>()
                .ForMember(dest => dest.OfficialRate, opt => opt.MapFrom(src => src.OfficialRate))
                .ForMember(dest => dest.RatePerUnit, opt => opt.MapFrom(src => src.RatePerUnit))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate));
        }
    }
}
