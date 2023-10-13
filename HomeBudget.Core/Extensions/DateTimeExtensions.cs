using System;
using System.Collections.Generic;

using HomeBudget.Core.Models;

namespace HomeBudget.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateOnly LastDateOfYear(this DateOnly currentDate)
            => DateOnly.FromDateTime(new DateTime(currentDate.Year + 1, 1, 1).AddDays(-1));

        public static DateOnly FirstDateOfYear(this DateOnly currentDate)
            => DateOnly.FromDateTime(new DateTime(currentDate.Year, 1, 1));

        public static IEnumerable<PeriodRange> GetFullYearsDateRangesForPeriod(this PeriodRange period)
        {
            var currentDate = period.StartDate;

            while (currentDate.Year < period.EndDate.Year - 1)
            {
                currentDate = currentDate.AddYears(1);

                yield return new PeriodRange
                {
                    StartDate = new DateOnly(currentDate.Year, 1, 1),
                    EndDate = new DateOnly(currentDate.Year, 12, 31)
                };
            }
        }
    }
}
