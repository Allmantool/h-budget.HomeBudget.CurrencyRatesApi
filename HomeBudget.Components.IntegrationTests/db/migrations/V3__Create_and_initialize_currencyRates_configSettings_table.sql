IF NOT EXISTS (SELECT * FROM sysobjects WITH (NOLOCK) WHERE NAME = N'ConfigSettings' and xtype = N'U')
BEGIN
    CREATE TABLE [HomeBudget].[dbo].[ConfigSettings] (
        Id INT PRIMARY KEY IDENTITY (1, 1),
		[Key] NVARCHAR(100),
        Settings NVARCHAR(Max),
    );
	
	DECLARE @defaultSettings AS NVARCHAR(Max) = 
		N'{' +
			N'"ActiveCurrencies" :' +
				N'[' +
					N'"USD","RUB","EUR","UAH","PLN","TRY"' +
				N']' +
		N'}';
	
	INSERT INTO [HomeBudget].[dbo].[ConfigSettings]
				( [Key], Settings )
		 VALUES ( N'General', @defaultSettings );
END