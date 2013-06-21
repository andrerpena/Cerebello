BEGIN TRANSACTION
GO
ALTER TABLE dbo.AccountContract
    ALTER COLUMN IssuanceDate datetime NOT NULL
ALTER TABLE dbo.AccountContract
	ALTER COLUMN StartDate datetime NOT NULL
ALTER TABLE dbo.AccountContract
	ALTER COLUMN EndDate datetime NULL
GO


ALTER TABLE dbo.Billing
	ALTER COLUMN IssuanceDate datetime NOT NULL
ALTER TABLE dbo.Billing
	ALTER COLUMN DueDate datetime NOT NULL
ALTER TABLE dbo.Billing
	ALTER COLUMN ReferenceDate datetime NOT NULL
ALTER TABLE dbo.Billing
	ALTER COLUMN ReferenceDateEnd datetime NULL
ALTER TABLE dbo.Billing
	ALTER COLUMN PaymentDate datetime NULL
GO


COMMIT
