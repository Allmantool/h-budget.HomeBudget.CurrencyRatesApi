﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.DataAccess.Constants;
using HomeBudget.DataAccess.Extensions;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    public class CurrencyRatesWriteProvider(
        IMapper mapper,
        IBaseWriteRepository writeRepository)
        : ICurrencyRatesWriteProvider
    {
        public async Task<int> UpsertRatesWithSaveAsync(IReadOnlyCollection<CurrencyRate> rates)
        {
            using var upsertTransaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var affectedRowsPerRequests = await UpsertRatesRequestAsync(rates).ToListAsync();

            upsertTransaction.Complete();

            return affectedRowsPerRequests.Sum();
        }

        private async IAsyncEnumerable<int> UpsertRatesRequestAsync(IEnumerable<CurrencyRate> rates)
        {
            var dbEntities = mapper.Map<IEnumerable<CurrencyRateEntity>>(rates);

            var dt = dbEntities.ToDataTable();

            var mergeQuery = $@"
                MERGE INTO [dbo].[CurrencyRates] AS TRG 
                     USING @{dt.TableName} AS SRC
                        ON
                              TRG.CurrencyId = SRC.CurrencyId
                          AND TRG.UpdateDate = SRC.UpdateDate
                          
                WHEN MATCHED THEN
                UPDATE
                   SET TRG.OfficialRate = SRC.OfficialRate,
                       TRG.RatePerUnit = SRC.RatePerUnit
                WHEN NOT MATCHED THEN
                INSERT ([CurrencyId], [Name], [Abbreviation], [Scale], [OfficialRate], [RatePerUnit], [UpdateDate]) 
                VALUES (SRC.CurrencyId, SRC.Name, SRC.Abbreviation, SRC.Scale, SRC.OfficialRate, SRC.RatePerUnit, SRC.UpdateDate);
                ";

            yield return await writeRepository.ExecuteAsync(mergeQuery, dt, TableTypes.CurrencyRateEntityType);
        }
    }
}
