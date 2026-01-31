using System;
using System.Linq;

namespace HomeBudget.Rates.Api.Monitoring
{
    internal sealed class ServiceProviderTracer : IDisposable
    {
        public void Dispose()
        {
            Console.WriteLine("🔥 IServiceProvider DISPOSED");
            Environment.StackTrace
                .Split(Environment.NewLine)
                .ToList()
                .ForEach(Console.WriteLine);
        }
    }
}
