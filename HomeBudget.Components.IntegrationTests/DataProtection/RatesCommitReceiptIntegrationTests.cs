using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HomeBudget.Components.CurrencyRates.DataProtection;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Components.IntegrationTests.WebApps;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace HomeBudget.Components.IntegrationTests.DataProtection
{
    [TestFixture]
    [Category(TestTypes.Integration)]
    [NonParallelizable]
    internal sealed class RatesCommitReceiptIntegrationTests : BaseIntegrationTests
    {
        private readonly CurrencyRatesTestWebApp _webApp = new();

        [SetUp]
        public override async Task SetupAsync()
        {
            await _webApp.InitAsync();
            await EnsureMaintenanceEpochFixtureAsync();
            await ClearMaintenanceEpochFixtureAsync();
        }

        [TearDown]
        public override async Task TerminateAsync()
        {
            await ClearMaintenanceEpochFixtureAsync();
            await _webApp.DisposeAsync();
        }

        [Test]
        public async Task RateAndSettingsMutations_WhenMaintenanceEpochIsClosed_AreRejectedWithoutReceiptsOrPartialWrites()
        {
            var updateDate = new DateOnly(2041, 2, 3);
            var settingsBefore = await GetStoredSettingsJsonAsync();
            var receiptBefore = await GetMaximumCommitSequenceAsync();
            await InsertMaintenanceEpochFixtureAsync();

            await using var scope = _webApp.Services.CreateAsyncScope();
            var ratesProvider = scope.ServiceProvider.GetRequiredService<ICurrencyRatesWriteProvider>();
            var settingsProvider = scope.ServiceProvider.GetRequiredService<IConfigSettingsProvider>();
            var frontierProvider = scope.ServiceProvider.GetRequiredService<IRatesCaptureFrontierProvider>();
            var frontier = await frontierProvider.AcquireAsync();
            var rateWrite = () => ratesProvider.UpsertRatesWithSaveAsync(
                new[]
                {
                    new HomeBudget.Components.CurrencyRates.Models.CurrencyRate
                    {
                        CurrencyId = 431,
                        Name = "US Dollar",
                        Abbreviation = "USD",
                        Scale = 1,
                        OfficialRate = 7.25m,
                        RatePerUnit = 7.25m,
                        UpdateDate = updateDate
                    }
                });
            var settingsWrite = () => settingsProvider.SaveDefaultSettingsAsync(
                new ConfigSettings { ActiveNationalBankCurrencies = Array.Empty<ConfigCurrency>() });

            await rateWrite.Should().ThrowAsync<SqlException>();
            await settingsWrite.Should().ThrowAsync<SqlException>();
            (await CountRatesAsync(updateDate)).Should().Be(0);
            (await GetStoredSettingsJsonAsync()).Should().Be(settingsBefore);
            frontier.CommitSequence.Should().Be(receiptBefore + 1);
            (await GetMaximumCommitSequenceAsync()).Should().Be(frontier.CommitSequence);
        }

        [Test]
        public async Task RateMutation_WhenExactMaintenanceEpochIsReleased_ResumesNormally()
        {
            var updateDate = new DateOnly(2041, 2, 4);
            var epochId = await InsertMaintenanceEpochFixtureAsync();
            await ReleaseMaintenanceEpochFixtureAsync(epochId);

            await using var scope = _webApp.Services.CreateAsyncScope();
            var provider = scope.ServiceProvider.GetRequiredService<ICurrencyRatesWriteProvider>();
            var affected = await provider.UpsertRatesWithSaveAsync(
                new[]
                {
                    new HomeBudget.Components.CurrencyRates.Models.CurrencyRate
                    {
                        CurrencyId = 431,
                        Name = "US Dollar",
                        Abbreviation = "USD",
                        Scale = 1,
                        OfficialRate = 7.5m,
                        RatePerUnit = 7.5m,
                        UpdateDate = updateDate
                    }
                });

            affected.Should().Be(1);
            (await CountRatesAsync(updateDate)).Should().Be(1);
        }

        [Test]
        public async Task AddRatesAsync_WhenMutationCommits_CreatesDurableRateReceipt()
        {
            var before = await GetMaximumCommitSequenceAsync();
            await using var scope = _webApp.Services.CreateAsyncScope();
            var provider = scope.ServiceProvider.GetRequiredService<ICurrencyRatesWriteProvider>();
            var affectedRows = await provider.UpsertRatesWithSaveAsync(
                new[]
                {
                    new HomeBudget.Components.CurrencyRates.Models.CurrencyRate
                    {
                        Scale = 1,
                        Abbreviation = "USD",
                        CurrencyId = 431,
                        Name = "US Dollar",
                        OfficialRate = 3.1415926535897932384626433832m,
                        RatePerUnit = 3.1415926535897932384626433832m,
                        UpdateDate = new DateOnly(2030, 1, 2)
                    }
                });

            var receipt = await GetLatestReceiptAsync("rates");

            affectedRows.Should().Be(1);
            receipt.CommitSequence.Should().BeGreaterThan(before);
            receipt.AffectedRows.Should().Be(1);
            receipt.CommittedUtc.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public async Task SaveSettingsAsync_WhenMutationCommits_CreatesDurableSettingsReceipt()
        {
            var before = await GetMaximumCommitSequenceAsync();
            await using var scope = _webApp.Services.CreateAsyncScope();
            var provider = scope.ServiceProvider.GetRequiredService<IConfigSettingsProvider>();
            var affectedRows = await provider.SaveDefaultSettingsAsync(
                new ConfigSettings
                {
                    ActiveNationalBankCurrencies = new[]
                    {
                        new ConfigCurrency
                        {
                            Id = 431,
                            Name = "US Dollar",
                            Scale = 1,
                            Abbreviation = "USD",
                            Periodicity = 0
                        }
                    }
                });

            var receipt = await GetLatestReceiptAsync("settings");

            affectedRows.Should().Be(1);
            receipt.CommitSequence.Should().BeGreaterThan(before);
            receipt.AffectedRows.Should().Be(1);
        }

        [Test]
        public async Task AcquireAsync_WhenConcurrentWithRateWrites_ReturnsSequenceInSingleCommitOrder()
        {
            var before = await GetMaximumCommitSequenceAsync();
            var writeTasks = Enumerable.Range(0, 8)
                .Select(WriteRateAsync)
                .ToArray();
            var frontierTasks = Enumerable.Range(0, 8)
                .Select(_ => AcquireFrontierAsync(_webApp))
                .ToArray();

            await Task.WhenAll(writeTasks.Cast<Task>().Concat(frontierTasks));

            var frontiers = await Task.WhenAll(frontierTasks);
            var receipts = await GetReceiptsAfterAsync(before);

            receipts.Should().HaveCount(16);
            receipts.Select(receipt => receipt.CommitSequence).Should().OnlyHaveUniqueItems();
            receipts.Select(receipt => receipt.CommitSequence).Should().BeInAscendingOrder();

            foreach (var frontier in frontiers)
            {
                receipts.Should().Contain(receipt =>
                    receipt.ReceiptId == frontier.ReceiptId
                    && receipt.CommitSequence == frontier.CommitSequence
                    && receipt.MutationKind == "fence");
                receipts.Should().OnlyContain(receipt =>
                    receipt.CommitSequence < frontier.CommitSequence
                    || receipt.CommitSequence > frontier.CommitSequence
                    || receipt.ReceiptId == frontier.ReceiptId);
            }
        }

        [Test]
        public async Task AcquireAsync_WhenApplicationHostIsRecreated_PreservesPriorFrontier()
        {
            var first = await AcquireFrontierAsync(_webApp);
            await using var restartedApp = new CurrencyRatesTestWebApp();

            try
            {
                await restartedApp.InitAsync();
                var second = await AcquireFrontierAsync(restartedApp);
                var persisted = await GetReceiptAsync(first.ReceiptId);

                second.CommitSequence.Should().BeGreaterThan(first.CommitSequence);
                persisted.CommitSequence.Should().Be(first.CommitSequence);
                persisted.MutationKind.Should().Be("fence");
            }
            finally
            {
                await restartedApp.ShutdownAsync();
            }
        }

        [Test]
        public async Task AcquireAsync_WhenCancellationAlreadyRequested_DoesNotCreateReceipt()
        {
            var before = await GetMaximumCommitSequenceAsync();
            using var cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();

            await using var scope = _webApp.Services.CreateAsyncScope();
            var provider = scope.ServiceProvider.GetRequiredService<IRatesCaptureFrontierProvider>();
            var acquire = () => provider.AcquireAsync(cancellation.Token);

            await acquire.Should().ThrowAsync<OperationCanceledException>();
            (await GetMaximumCommitSequenceAsync()).Should().Be(before);
        }

        [Test]
        public async Task CaptureFrontier_WhenSerialized_DeclaresInvariantStringsAndLegacyFloatLimitation()
        {
            var frontier = await AcquireFrontierAsync(_webApp);
            var json = JsonSerializer.Serialize(frontier);
            var persistedTypes = await GetRateNumericSqlTypesAsync();

            json.Should().Contain("\"DecimalSerialization\":\"invariant-decimal-string\"");
            json.Should().Contain("\"SourceDecimalStorage\":\"legacy-sql-float-lossy\"");
            persistedTypes.Should().Equal("float", "float", "float");
        }

        [Test]
        public async Task RateMutation_WhenReceiptInsertFails_RollsBackAuthoritativeWrite()
        {
            var updateDate = new DateOnly(2040, 1, 1);
            await CreateRejectReceiptTriggerAsync();

            try
            {
                await using var scope = _webApp.Services.CreateAsyncScope();
                var provider = scope.ServiceProvider.GetRequiredService<ICurrencyRatesWriteProvider>();
                var write = () => provider.UpsertRatesWithSaveAsync(
                    new[]
                    {
                        new HomeBudget.Components.CurrencyRates.Models.CurrencyRate
                        {
                            CurrencyId = 431,
                            Name = "US Dollar",
                            Abbreviation = "USD",
                            Scale = 1,
                            OfficialRate = 9.87654321m,
                            RatePerUnit = 9.87654321m,
                            UpdateDate = updateDate
                        }
                    });

                await write.Should().ThrowAsync<SqlException>();
                (await CountRatesAsync(updateDate)).Should().Be(0);
            }
            finally
            {
                await DropRejectReceiptTriggerAsync();
            }
        }

        [Test]
        public async Task SettingsMutation_WhenReceiptInsertFails_RollsBackAuthoritativeWrite()
        {
            var before = await GetStoredSettingsJsonAsync();
            await CreateRejectReceiptTriggerAsync();

            try
            {
                await using var scope = _webApp.Services.CreateAsyncScope();
                var provider = scope.ServiceProvider.GetRequiredService<IConfigSettingsProvider>();
                var write = () => provider.SaveDefaultSettingsAsync(new ConfigSettings
                {
                    ActiveNationalBankCurrencies = Array.Empty<ConfigCurrency>()
                });

                await write.Should().ThrowAsync<SqlException>();
                (await GetStoredSettingsJsonAsync()).Should().Be(before);
            }
            finally
            {
                await DropRejectReceiptTriggerAsync();
            }
        }

        private async Task<long> GetMaximumCommitSequenceAsync()
        {
            const string query = """
                SELECT COALESCE(MAX([CommitSequence]), 0)
                FROM [HomeBudget.CurrencyRates].[dbo].[RatesCommitReceipts];
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();

            return (long)await command.ExecuteScalarAsync();
        }

        private async Task<CommitReceiptRow> GetLatestReceiptAsync(string mutationKind)
        {
            const string query = """
                SELECT TOP (1)
                       [CommitSequence],
                       [AffectedRows],
                       [CommittedUtc]
                FROM [HomeBudget.CurrencyRates].[dbo].[RatesCommitReceipts]
                WHERE [MutationKind] = @MutationKind
                ORDER BY [CommitSequence] DESC;
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@MutationKind", SqlDbType.NVarChar, 16).Value = mutationKind;
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            (await reader.ReadAsync()).Should().BeTrue();

            return new CommitReceiptRow(
                reader.GetInt64(0),
                reader.GetInt32(1),
                DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc));
        }

        private async Task WriteRateAsync(int offset)
        {
            await using var scope = _webApp.Services.CreateAsyncScope();
            var provider = scope.ServiceProvider.GetRequiredService<ICurrencyRatesWriteProvider>();
            var value = 3.5m + (offset / 100m);

            await provider.UpsertRatesWithSaveAsync(
                new[]
                {
                    new HomeBudget.Components.CurrencyRates.Models.CurrencyRate
                    {
                        CurrencyId = 431,
                        Name = "US Dollar",
                        Abbreviation = "USD",
                        Scale = 1,
                        OfficialRate = value,
                        RatePerUnit = value,
                        UpdateDate = new DateOnly(2031, 1, 1).AddDays(offset)
                    }
                });
        }

        private static async Task<RatesCaptureFrontier> AcquireFrontierAsync(
            CurrencyRatesTestWebApp webApp)
        {
            await using var scope = webApp.Services.CreateAsyncScope();
            var provider = scope.ServiceProvider.GetRequiredService<IRatesCaptureFrontierProvider>();

            return await provider.AcquireAsync();
        }

        private async Task<IReadOnlyCollection<ReceiptOrderRow>> GetReceiptsAfterAsync(long commitSequence)
        {
            const string query = """
                SELECT ReceiptId, CommitSequence, MutationKind
                FROM dbo.RatesCommitReceipts
                WHERE CommitSequence > @CommitSequence
                ORDER BY CommitSequence;
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@CommitSequence", SqlDbType.BigInt).Value = commitSequence;
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            var receipts = new List<ReceiptOrderRow>();

            while (await reader.ReadAsync())
            {
                receipts.Add(new ReceiptOrderRow(
                    reader.GetGuid(0),
                    reader.GetInt64(1),
                    reader.GetString(2)));
            }

            return receipts;
        }

        private async Task<ReceiptOrderRow> GetReceiptAsync(Guid receiptId)
        {
            const string query = """
                SELECT ReceiptId, CommitSequence, MutationKind
                FROM dbo.RatesCommitReceipts
                WHERE ReceiptId = @ReceiptId;
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@ReceiptId", SqlDbType.UniqueIdentifier).Value = receiptId;
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            (await reader.ReadAsync()).Should().BeTrue();

            return new ReceiptOrderRow(
                reader.GetGuid(0),
                reader.GetInt64(1),
                reader.GetString(2));
        }

        private async Task<IReadOnlyCollection<string>> GetRateNumericSqlTypesAsync()
        {
            const string query = """
                SELECT TYPE_NAME(system_type_id)
                FROM sys.columns
                WHERE object_id = OBJECT_ID(N'dbo.CurrencyRates')
                  AND name IN (N'Scale', N'OfficialRate', N'RatePerUnit')
                ORDER BY column_id;
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            var types = new List<string>();

            while (await reader.ReadAsync())
            {
                types.Add(reader.GetString(0));
            }

            return types;
        }

        private async Task<int> CountRatesAsync(DateOnly updateDate)
        {
            const string query = """
                SELECT COUNT(*)
                FROM dbo.CurrencyRates
                WHERE CurrencyId = 431
                  AND UpdateDate = @UpdateDate;
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@UpdateDate", SqlDbType.Date).Value = updateDate;
            await connection.OpenAsync();

            return (int)await command.ExecuteScalarAsync();
        }

        private async Task<string> GetStoredSettingsJsonAsync()
        {
            const string query = """
                SELECT Settings
                FROM dbo.ConfigSettings
                WHERE [Key] = N'General';
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();

            return (string)await command.ExecuteScalarAsync();
        }

        private async Task CreateRejectReceiptTriggerAsync()
        {
            const string query = """
                CREATE OR ALTER TRIGGER dbo.TR_TestRejectRatesCommitReceipt
                ON dbo.RatesCommitReceipts
                INSTEAD OF INSERT
                AS
                    THROW 51002, 'Injected receipt failure.', 1;
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private async Task DropRejectReceiptTriggerAsync()
        {
            const string query = """
                DROP TRIGGER IF EXISTS dbo.TR_TestRejectRatesCommitReceipt;
                """;

            await using var connection = CreateConnection();
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private SqlConnection CreateConnection()
        {
            var builder = new SqlConnectionStringBuilder(TestContainers.MsSqlDbContainer.GetConnectionString())
            {
                InitialCatalog = "HomeBudget.CurrencyRates"
            };

            return new SqlConnection(builder.ConnectionString);
        }

        private async Task EnsureMaintenanceEpochFixtureAsync()
        {
            const string query = """
                IF DB_ID(N'HomeBudget.DataProtection') IS NULL
                    CREATE DATABASE [HomeBudget.DataProtection];

                IF OBJECT_ID(N'[HomeBudget.DataProtection].dbo.MaintenanceEpochs', N'U') IS NULL
                BEGIN
                    EXEC(N'CREATE TABLE [HomeBudget.DataProtection].dbo.MaintenanceEpochs
                    (
                        EpochId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                        State NVARCHAR(16) NOT NULL
                    );');
                END;
                """;
            await using var connection = new SqlConnection(TestContainers.MsSqlDbContainer.GetConnectionString());
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private async Task ClearMaintenanceEpochFixtureAsync()
        {
            const string query = "DELETE FROM [HomeBudget.DataProtection].dbo.MaintenanceEpochs;";
            await using var connection = new SqlConnection(TestContainers.MsSqlDbContainer.GetConnectionString());
            await using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private async Task<Guid> InsertMaintenanceEpochFixtureAsync()
        {
            var epochId = Guid.NewGuid();
            const string query = "INSERT INTO [HomeBudget.DataProtection].dbo.MaintenanceEpochs (EpochId, State) VALUES (@EpochId, N'closed');";
            await using var connection = new SqlConnection(TestContainers.MsSqlDbContainer.GetConnectionString());
            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@EpochId", SqlDbType.UniqueIdentifier).Value = epochId;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return epochId;
        }

        private async Task ReleaseMaintenanceEpochFixtureAsync(Guid epochId)
        {
            const string query = "UPDATE [HomeBudget.DataProtection].dbo.MaintenanceEpochs SET State = N'released' WHERE EpochId = @EpochId AND State = N'closed';";
            await using var connection = new SqlConnection(TestContainers.MsSqlDbContainer.GetConnectionString());
            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@EpochId", SqlDbType.UniqueIdentifier).Value = epochId;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private sealed record CommitReceiptRow(
            long CommitSequence,
            int AffectedRows,
            DateTime CommittedUtc);

        private sealed record ReceiptOrderRow(
            Guid ReceiptId,
            long CommitSequence,
            string MutationKind);
    }
}
