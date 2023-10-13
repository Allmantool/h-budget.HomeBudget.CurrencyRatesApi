using System;

namespace HomeBudget.Core.Models
{
    public class PeriodRange
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public bool IsWithinTheSameYear() => StartDate.Year == EndDate.Year;
    }
}
