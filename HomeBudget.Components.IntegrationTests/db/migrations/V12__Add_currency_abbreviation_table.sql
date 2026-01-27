IF NOT EXISTS (SELECT * FROM sysobjects WITH (NOLOCK) WHERE [name]=N'CurrencyAbbreviations' and xtype=N'U')
BEGIN
    CREATE TABLE [HomeBudget.CurrencyRates].[dbo].[CurrencyAbbreviations] (
		    CurrencyId INT,
			[Name] NVARCHAR(100),
		    Abbreviation NVARCHAR(3)
    );
END

INSERT INTO [HomeBudget.CurrencyRates].[dbo].[CurrencyAbbreviations] 
            ([CurrencyId], [Name], [Abbreviation])
     VALUES (456, N'Российских рублей', N'RUB'),
            (460, N'Турецких лир', N'TRY'),
            (451, N'Евро', N'EUR'),
            (431, N'Доллар США', N'USD'),
            (449, N'Гривен', N'UAH'),
            (452, N'Злотых', N'PLN');
GO