using FluentValidation;

using HomeBudget.Rates.Api.Models;

namespace HomeBudget.Rates.Api.Validators
{
    public class CurrencySaveRatesRequestValidator : AbstractValidator<CurrencySaveRatesRequest>
    {
        public CurrencySaveRatesRequestValidator() => RuleForEach(x => x.CurrencyRates)
            .SetValidator(new CurrencyRateValidator());
    }
}
