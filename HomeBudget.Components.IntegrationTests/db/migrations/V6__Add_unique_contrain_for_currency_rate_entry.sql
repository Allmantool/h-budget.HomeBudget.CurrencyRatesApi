USE master;
GO

IF EXISTS(SELECT * FROM sys.databases WITH (NOLOCK) WHERE name = N'HomeBudget')
BEGIN

	ALTER TABLE [HomeBudget.CurrencyRates].[dbo].[CurrencyRates] 
	ADD CONSTRAINT constraintname UNIQUE ([CurrencyId],[UpdateDate]);
	
END