using System;
using System.Threading;
using System.Threading.Tasks;

using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.DataProtection
{
    internal sealed class RatesCaptureFrontierProvider : IRatesCaptureFrontierProvider
    {
        private const string AcquireFrontierSql = RatesCommitSql.BeginTransaction + """
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
                    (@ReceiptId, N'fence', 0, SYSUTCDATETIME());

                COMMIT TRANSACTION;

                SELECT ReceiptId, CommitSequence, AffectedRows, CommittedUtc
                FROM @Receipt;
            """ + RatesCommitSql.RollbackOnError;

        private readonly IBaseWriteRepository _writeRepository;

        public RatesCaptureFrontierProvider(IBaseWriteRepository writeRepository)
        {
            _writeRepository = writeRepository;
            _writeRepository.Database = RatesCommitSql.DatabaseName;
        }

        public async Task<RatesCaptureFrontier> AcquireAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _writeRepository.QuerySingleAsync<RatesCommitWriteResult, RatesCommitPayload>(
                AcquireFrontierSql,
                new RatesCommitPayload
                {
                    ReceiptId = Guid.NewGuid(),
                    LockResource = RatesCommitSql.LockResource,
                    LockTimeoutMilliseconds = RatesCommitSql.LockTimeoutMilliseconds
                },
                cancellationToken);

            return new RatesCaptureFrontier(
                result.ReceiptId,
                result.CommitSequence,
                new DateTimeOffset(DateTime.SpecifyKind(result.CommittedUtc, DateTimeKind.Utc)));
        }
    }
}
