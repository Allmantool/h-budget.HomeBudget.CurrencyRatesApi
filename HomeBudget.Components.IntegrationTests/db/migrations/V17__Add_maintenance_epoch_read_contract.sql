-- evolve-tx-off
USE master;
GO

IF DB_ID(N'HomeBudget.DataProtection') IS NULL
BEGIN
    CREATE DATABASE [HomeBudget.DataProtection];
END;
GO

USE [HomeBudget.DataProtection];
GO

-- The Rates service has a read-only dependency on the shared maintenance fence.
-- The authoritative production schema is owned by root migration V27; this
-- integration-only fixture intentionally models only the columns Rates reads.
IF OBJECT_ID(N'dbo.MaintenanceEpochs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MaintenanceEpochs
    (
        EpochId UNIQUEIDENTIFIER NOT NULL,
        State NVARCHAR(16) NOT NULL,
        CONSTRAINT PK_RatesIntegrationMaintenanceEpochs PRIMARY KEY (EpochId),
        CONSTRAINT CK_RatesIntegrationMaintenanceEpochs_State CHECK
            (State IN (N'closed', N'released', N'failed'))
    );
END;
GO

USE [HomeBudget.CurrencyRates];
GO
