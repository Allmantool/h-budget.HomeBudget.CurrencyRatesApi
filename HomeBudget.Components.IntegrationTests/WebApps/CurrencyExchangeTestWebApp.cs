using System.Threading.Tasks;

namespace HomeBudget.Components.IntegrationTests.WebApps
{
    internal class CurrencyExchangeTestWebApp : BaseTestWebApp<Program>
    {
        protected override ValueTask DisposeAsyncCoreAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
