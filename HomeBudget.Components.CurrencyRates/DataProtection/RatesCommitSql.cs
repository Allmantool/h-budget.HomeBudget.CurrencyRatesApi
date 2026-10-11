namespace HomeBudget.Components.CurrencyRates.DataProtection
{
    internal static class RatesCommitSql
    {
        public const string DatabaseName = "HomeBudget.CurrencyRates";
        public const string LockResource = "HomeBudget.Rates.CommitFence.v1";
        public const string GlobalMaintenanceLockResource = "HomeBudget.DataProtection.Admission.Global.v1";
        public const int LockTimeoutMilliseconds = 30_000;

        public const string BeginTransaction = """
            SET XACT_ABORT ON;
            BEGIN TRY
                BEGIN TRANSACTION;

                DECLARE @LockResult INT;
                EXEC @LockResult = sys.sp_getapplock
                    @Resource = @LockResource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = @LockTimeoutMilliseconds;

                IF @LockResult < 0
                    THROW 51001, 'Unable to acquire the Rates commit fence.', 1;
            """;

        public const string BeginMutationTransaction = BeginTransaction + """
                EXEC @LockResult = sys.sp_getapplock
                    @Resource = @GlobalMaintenanceLockResource,
                    @LockMode = 'Shared',
                    @LockOwner = 'Transaction',
                    @LockTimeout = @LockTimeoutMilliseconds;

                IF @LockResult < 0
                    THROW 51030, 'Unable to acquire the global maintenance admission permit.', 1;

                IF EXISTS
                (
                    SELECT 1
                    FROM [HomeBudget.DataProtection].dbo.MaintenanceEpochs
                    WHERE State = N'closed'
                )
                    THROW 51031, 'Rates mutations are temporarily closed for maintenance.', 1;
            """;

        public const string RollbackOnError = """
            END TRY
            BEGIN CATCH
                IF XACT_STATE() <> 0
                    ROLLBACK TRANSACTION;

                THROW;
            END CATCH;
            """;
    }
}
