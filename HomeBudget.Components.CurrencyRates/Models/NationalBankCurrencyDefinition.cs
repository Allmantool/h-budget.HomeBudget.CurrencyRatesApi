using System;

namespace HomeBudget.Components.CurrencyRates.Models
{
    internal sealed record NationalBankCurrencyDefinition(
        int CurrencyId,
        int? ParentId,
        string Code,
        string Abbreviation,
        string Name,
        string EnglishName,
        int Scale,
        int Periodicity,
        DateOnly DateStart,
        DateOnly DateEnd);
}
