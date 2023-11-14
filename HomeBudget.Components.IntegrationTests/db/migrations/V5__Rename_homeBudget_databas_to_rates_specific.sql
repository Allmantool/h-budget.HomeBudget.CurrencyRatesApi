-- evolve-tx-off
IF EXISTS(SELECT * FROM sys.databases WITH (NOLOCK) WHERE name = N'HomeBudget')
BEGIN
    ALTER DATABASE [HomeBudget] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [HomeBudget] MODIFY NAME = [HomeBudget.CurrencyRates]; 
    ALTER DATABASE [HomeBudget.CurrencyRates] SET MULTI_USER;
END
