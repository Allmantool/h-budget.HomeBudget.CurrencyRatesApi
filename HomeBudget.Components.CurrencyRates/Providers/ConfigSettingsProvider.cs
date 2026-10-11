using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.DataProtection;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class ConfigSettingsProvider : IConfigSettingsProvider
    {
        private const string GeneralConfigSettingsKey = "General";
        private const string SaveSettingsSql = RatesCommitSql.BeginMutationTransaction + """
                UPDATE dbo.ConfigSettings
                   SET Settings = @Settings
                 WHERE [Key] = @Key;

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
                    (@ReceiptId, N'settings', @AffectedRows, SYSUTCDATETIME());

                COMMIT TRANSACTION;

                SELECT ReceiptId, CommitSequence, AffectedRows, CommittedUtc
                FROM @Receipt;
            """ + RatesCommitSql.RollbackOnError;

        private readonly IBaseWriteRepository _writeRepository;
        private readonly IBaseReadRepository _readRepository;

        public ConfigSettingsProvider(
            IBaseWriteRepository writeRepository,
            IBaseReadRepository readRepository)
        {
            _writeRepository = writeRepository;
            _readRepository = readRepository;

            _readRepository.Database = RatesCommitSql.DatabaseName;
            _writeRepository.Database = RatesCommitSql.DatabaseName;
        }

        public async Task<ConfigSettings> GetDefaultSettingsAsync()
        {
            const string query = $"SELECT [Settings] FROM [dbo].[ConfigSettings] WITH (NOLOCK) WHERE [Key] = @Key";

            var configAsJson = await _readRepository.SingleAsync<string>(
                query,
                new
                {
                    Key = GeneralConfigSettingsKey
                });

            return JsonSerializer.Deserialize<ConfigSettings>(configAsJson);
        }

        public async Task<int> SaveDefaultSettingsAsync(
            ConfigSettings settings,
            CancellationToken cancellationToken = default)
        {
            var settingsAsJson = JsonSerializer.Serialize(settings);
            var receipt = await _writeRepository.QuerySingleAsync<RatesCommitWriteResult, SettingsForUpdatePayload>(
                SaveSettingsSql,
                new SettingsForUpdatePayload
                {
                    Key = GeneralConfigSettingsKey,
                    Settings = settingsAsJson,
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
