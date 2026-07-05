USE [HomeBudget.CurrencyRates];
GO

MERGE [dbo].[CurrencyAbbreviations] AS target
USING
(
    VALUES
        (431, N'Доллар США', N'USD'),
        (456, N'Российских рублей', N'RUB'),
        (451, N'Евро', N'EUR'),
        (449, N'Гривен', N'UAH'),
        (452, N'Злотых', N'PLN'),
        (460, N'Турецких лир', N'TRY'),
        (462, N'Китайский юань', N'CNY'),
        (468, N'Таиландский бат', N'THB')
) AS source ([CurrencyId], [Name], [Abbreviation])
ON target.[CurrencyId] = source.[CurrencyId]
WHEN MATCHED THEN
    UPDATE
       SET target.[Name] = source.[Name],
           target.[Abbreviation] = source.[Abbreviation]
WHEN NOT MATCHED THEN
    INSERT ([CurrencyId], [Name], [Abbreviation])
    VALUES (source.[CurrencyId], source.[Name], source.[Abbreviation]);
GO

DECLARE @GeneralKey AS NVARCHAR(15) = N'General';

DECLARE @defaultSettings AS NVARCHAR(MAX) =
    N'{' +
        N'"ActiveNationalBankCurrencies" :' +
            N'[' +
                N'{"Abbreviation":"USD", "Id":431, "Name": "US Dollar", "Scale": 1},' +
                N'{"Abbreviation":"RUB", "Id":456, "Name": "Russian Ruble", "Scale": 100},' +
                N'{"Abbreviation":"EUR", "Id":451, "Name": "Euro", "Scale": 1},' +
                N'{"Abbreviation":"UAH", "Id":449, "Name": "Hryvnia", "Scale": 100},' +
                N'{"Abbreviation":"PLN", "Id":452, "Name": "Polish Zloty", "Scale": 10},' +
                N'{"Abbreviation":"TRY", "Id":460, "Name": "Turkish Lira", "Scale": 10},' +
                N'{"Abbreviation":"CNY", "Id":462, "Name": "Yuan Renminbi", "Scale": 10},' +
                N'{"Abbreviation":"THB", "Id":468, "Name": "Baht", "Scale": 10}' +
            N']' +
    N'}';

UPDATE [dbo].[ConfigSettings]
   SET [Settings] = @defaultSettings
 WHERE [Key] = @GeneralKey;
GO
