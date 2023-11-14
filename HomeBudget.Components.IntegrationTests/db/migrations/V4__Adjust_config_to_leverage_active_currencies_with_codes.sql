DECLARE @GeneralKey AS NVARCHAR(15) = N'General';

DECLARE @defaultSettings AS NVARCHAR(Max) = 
    N'{' +
        N'"ActiveNationalBankCurrencies" :' +
            N'[' +
                N'{"Abbreviation":"USD", "Id":431, "Name": "US Dollar", "Scale": 1},' +
                N'{"Abbreviation":"RUB", "Id":190, "Name": "Russian Ruble", "Scale": 1},' +
                N'{"Abbreviation":"EUR", "Id":19, "Name": "EURO", "Scale": 1 },' +
                N'{"Abbreviation":"UAH", "Id":169, "Name": "Hryvnia", "Scale": 1 },' +
                N'{"Abbreviation":"PLN", "Id":188, "Name": "Polish Zloty", "Scale": 1 },' +
                N'{"Abbreviation":"TRY", "Id":220, "Name": "New Turkish Lira", "Scale": 1 }' +
            N']' +
    N'}';

IF EXISTS (SELECT * FROM [HomeBudget].[dbo].[ConfigSettings] WITH (NOLOCK) WHERE [Key] = @GeneralKey)
BEGIN
    DELETE FROM [HomeBudget].[dbo].[ConfigSettings] WHERE [Key] = @GeneralKey;
END

INSERT INTO [HomeBudget].[dbo].[ConfigSettings]
            ( [Key], Settings )
     VALUES ( @GeneralKey, @defaultSettings );
