using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    public class CurrencyRatesWriteProvider(
        IMapper mapper,
        IBaseWriteRepository writeRepository)
        : ICurrencyRatesWriteProvider
    {
        private const int MaxRatePerAnOperation = 300;

        public async Task<int> UpsertRatesWithSaveAsync(IReadOnlyCollection<CurrencyRate> rates)
        {
            using var upsertTransaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var affectedRowsPerRequests = await UpsertRatesRequestAsync(rates).ToListAsync();

            upsertTransaction.Complete();

            return affectedRowsPerRequests.Sum();
        }

        private async IAsyncEnumerable<int> UpsertRatesRequestAsync(IEnumerable<CurrencyRate> rates)
        {
            const string insertQuery = "INSERT INTO dbo.[CurrencyRates] " +
                                       "       ([CurrencyId], [Name], [Abbreviation], [Scale], [OfficialRate], [RatePerUnit], [UpdateDate]) " +
                                       "VALUES (@CurrencyId, @Name, @Abbreviation, @Scale, @OfficialRate, @RatePerUnit, @UpdateDate);";

            const string deleteQuery = "DELETE " +
                                       "  FROM dbo.[CurrencyRates] " +
                                       " WHERE [CurrencyId] IN @CurrencyIds AND [UpdateDate] IN @UpdateDates;";

            var dbEntities = mapper.Map<IEnumerable<CurrencyRateEntity>>(rates);

            foreach (var ratesPerAnChunk in dbEntities.Chunk(MaxRatePerAnOperation))
            {
                await writeRepository.ExecuteAsync(
                    deleteQuery,
                    new RatesForDeletePayload
                    {
                        CurrencyIds = ratesPerAnChunk.Select(r => r.CurrencyId),
                        UpdateDates = ratesPerAnChunk.Select(r => r.UpdateDate)
                    });

                yield return await writeRepository.ExecuteAsync(insertQuery, ratesPerAnChunk);
            }
        }
    }
}
