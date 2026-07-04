using System;

namespace HomeBudget.Components.CurrencyRates.Exceptions
{
    public sealed class NationalBankCurrencyResolutionException : Exception
    {
        public NationalBankCurrencyResolutionException(string message)
            : base(message)
        {
        }

        public NationalBankCurrencyResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NationalBankCurrencyResolutionException()
        {
        }
    }
}
