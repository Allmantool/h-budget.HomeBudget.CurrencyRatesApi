using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.DataProtection;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.DataAccess.Constants;
using HomeBudget.DataAccess.Extensions;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class CurrencyRatesWriteProvider : ICurrencyRatesWriteProvider
    {
        private const string UpsertRatesSql = RatesCommitSql.BeginMutationTransaction + """
                MERGE INTO dbo.CurrencyRates AS TRG
                USING @CurrencyRateEntity AS SRC
                   ON TRG.CurrencyId = SRC.CurrencyId
                  AND TRG.UpdateDate = SRC.UpdateDate
                WHEN MATCHED THEN
                    UPDATE SET
                        TRG.OfficialRate = SRC.OfficialRate,
                        TRG.RatePerUnit = SRC.RatePerUnit
                WHEN NOT MATCHED THEN
                    INSERT
                        (CurrencyId, Name, Abbreviation, Scale, OfficialRate, RatePerUnit, UpdateDate)
                    VALUES
                        (SRC.CurrencyId, SRC.Name, SRC.Abbreviation, SRC.Scale,
                         SRC.OfficialRate, SRC.RatePerUnit, SRC.UpdateDate);

                DECLARE @AffectedRows INT = @@ROWCOUNT;
                DECLARE @Receipt TABLE
                (
                    ReceiptId UNIQUEIDENTIFIER,
                    CommitSequence BIGINT,
                    AffectedRows INT,
                    CommittedUtc DATETIME2(7)
                );

                INSERT INTO dbo.RatesCommitReceipts
                    (ReceiptId, MutationKind, AffectedRows, CommittedUtc)
                OUTPUT inserted.ReceiptId,
                       inserted.CommitSequence,
                       inserted.AffectedRows,
                       inserted.CommittedUtc
                    INTO @Receipt
                VALUES
                    (@ReceiptId, N'rates', @AffectedRows, SYSUTCDATETIME());

                COMMIT TRANSACTION;

                SELECT ReceiptId, CommitSequence, AffectedRows, CommittedUtc
                FROM @Receipt;
            """ + RatesCommitSql.RollbackOnError;

        private readonly IMapper _mapper;
        private readonly IBaseWriteRepository _writeRepository;

        public CurrencyRatesWriteProvider(
            IMapper mapper,
            IBaseWriteRepository writeRepository)
        {
            _mapper = mapper;
            _writeRepository = writeRepository;

            writeRepository.Database = RatesCommitSql.DatabaseName;
        }

        public async Task<int> UpsertRatesWithSaveAsync(
            IReadOnlyCollection<CurrencyRate> rates,
            CancellationToken cancellationToken = default)
        {
            var dbEntities = _mapper.Map<IEnumerable<CurrencyRateEntity>>(rates);
            var dt = dbEntities.ToDataTable();
            var receipt = await _writeRepository.QuerySingleAsync<RatesCommitWriteResult>(
                UpsertRatesSql,
                dt,
                TableTypes.CurrencyRateEntityType,
                new RatesCommitPayload
                {
                    ReceiptId = Guid.NewGuid(),
                    LockResource = RatesCommitSql.LockResource,
                    GlobalMaintenanceLockResource = RatesCommitSql.GlobalMaintenanceLockResource,
                    LockTimeoutMilliseconds = RatesCommitSql.LockTimeoutMilliseconds
                },
                cancellationToken);

            return receipt.AffectedRows;
        }
    }
}
