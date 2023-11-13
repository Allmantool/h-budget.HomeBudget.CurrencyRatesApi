IF NOT EXISTS (SELECT * FROM sysobjects WITH (NOLOCK) WHERE [name]=N'CurrencyRates' and xtype=N'U')
BEGIN
    CREATE TABLE [HomeBudget].[dbo].[CurrencyRates] (
        Id INT PRIMARY KEY IDENTITY (1, 1),
		CurrencyId INT,
        Name NVARCHAR(100),
		Abbreviation NVARCHAR(3),
		Scale FLOAT,
		OfficialRate FLOAT,
		RatePerUnit FLOAT,
		UpdateDate Date
    )
END