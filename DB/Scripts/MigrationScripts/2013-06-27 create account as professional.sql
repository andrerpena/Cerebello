BEGIN TRANSACTION
GO




ALTER TABLE dbo.AccountContract
    ALTER COLUMN StartDate datetime NULL
GO




ALTER TABLE dbo.AccountContract ADD
    IsPartialBillingInfo bit NULL
GO
UPDATE dbo.AccountContract SET IsPartialBillingInfo = 0 WHERE IsPartialBillingInfo IS NULL
GO
ALTER TABLE dbo.AccountContract
    ALTER COLUMN IsPartialBillingInfo bit NOT NULL
GO




ALTER TABLE dbo.Billing ALTER COLUMN 
    ReferenceDate datetime NULL
GO
ALTER TABLE dbo.Practice ADD
    VerificationMethod varchar(10) NULL
GO
UPDATE dbo.Practice SET VerificationMethod = CASE WHEN Practice.VerificationDate IS NOT NULL THEN 'EMAIL' ELSE NULL END
GO
-- select * from Practice
ALTER TABLE dbo.Practice ADD
    AccountExpiryDate datetime NULL
GO
-- select * from glb_token
-- setting the AccountExpiryDate to the same date of the expiration of the validation token
UPDATE dbo.Practice SET AccountExpiryDate = CASE WHEN Practice.VerificationDate IS NOT NULL THEN NULL ELSE
    (select top 1 T.ExpirationDate from GLB_Token T
    where [Type] = 'VerifyPracticeAndEmail'
      AND Name   = 'Practice='+Practice.UrlIdentifier+'&UserName='+(select UserName from [user] U where Practice.OwnerId = U.Id))
    END
GO




COMMIT
