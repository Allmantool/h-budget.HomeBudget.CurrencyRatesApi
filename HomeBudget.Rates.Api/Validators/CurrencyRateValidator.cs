using FluentValidation;

using HomeBudget.Rates.Api.Models;

namespace HomeBudget.Rates.Api.Validators
{
    public class CurrencyRateValidator : AbstractValidator<CurrencyRate>
    {
        public CurrencyRateValidator()
        {
            RuleFor(x => x.RatePerUnit)
                .GreaterThan(default(decimal));

            RuleFor(x => x.Abbreviation)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.OfficialRate)
                .GreaterThan(default(decimal));

            RuleFor(x => x.Scale)
                .GreaterThan(default(int));
        }
    }
}
