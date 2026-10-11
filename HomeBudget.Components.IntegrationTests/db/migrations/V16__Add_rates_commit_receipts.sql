USE [HomeBudget.CurrencyRates];
GO

IF OBJECT_ID(N'dbo.RatesCommitReceipts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RatesCommitReceipts
    (
        CommitSequence BIGINT IDENTITY(1, 1) NOT NULL,
        ReceiptId UNIQUEIDENTIFIER NOT NULL,
        MutationKind NVARCHAR(16) NOT NULL,
        AffectedRows INT NOT NULL,
        CommittedUtc DATETIME2(7) NOT NULL,
        CONSTRAINT PK_RatesCommitReceipts PRIMARY KEY (CommitSequence),
        CONSTRAINT UX_RatesCommitReceipts_ReceiptId UNIQUE (ReceiptId),
        CONSTRAINT CK_RatesCommitReceipts_MutationKind CHECK
            (MutationKind IN (N'rates', N'settings', N'fence')),
        CONSTRAINT CK_RatesCommitReceipts_AffectedRows CHECK (AffectedRows >= 0)
    );

    CREATE INDEX IX_RatesCommitReceipts_KindSequence
        ON dbo.RatesCommitReceipts (MutationKind, CommitSequence DESC);
END;
GO
