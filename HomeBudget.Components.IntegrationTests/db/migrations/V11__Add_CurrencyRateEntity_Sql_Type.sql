USE [HomeBudget.CurrencyRates];
GO

IF TYPE_ID('CurrencyRateEntityType') IS NOT NULL
BEGIN
    DROP TYPE CurrencyRateEntityType;
END

-- Create the type again
CREATE TYPE CurrencyRateEntityType AS TABLE
(
    [CurrencyId] [int] NULL,
    [Name] [nvarchar](100) NULL,
    [Abbreviation] [nvarchar](3) NULL,
    [Scale] [float] NULL,
    [OfficialRate] [float] NULL,
    [RatePerUnit] [float] NULL,
    [UpdateDate] [date] NULL
);