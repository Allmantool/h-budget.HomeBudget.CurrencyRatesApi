using System.Collections.Generic;
using System.Linq;

namespace HomeBudget.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> origin)
        {
            return origin == null || !origin.Any();
        }
    }
}
