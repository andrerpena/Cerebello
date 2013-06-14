BEGIN TRANSACTION
GO

CREATE TABLE dbo.PatientFileGroup
	(
	Id int NOT NULL IDENTITY (1, 1),
	PatientId int NOT NULL,
	PracticeId int NOT NULL,
	GroupTitle nvarchar(100) NOT NULL,
	GroupNotes nvarchar(MAX) NULL,
	FileGroupDate datetime NOT NULL,
	ReceiveDate datetime NOT NULL
	)
GO
ALTER TABLE dbo.PatientFileGroup ADD CONSTRAINT
	PK_PatientFileGroup_1 PRIMARY KEY CLUSTERED 
	(
	Id
	)

GO




ALTER TABLE dbo.PatientFileGroup ADD CONSTRAINT
	FK_PatientFileGroup_Patient FOREIGN KEY
	(
	PatientId
	) REFERENCES dbo.Patient
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO

ALTER TABLE dbo.PatientFile ADD
	Title nvarchar(100) NULL,
	FileGroupId int NOT NULL

ALTER TABLE dbo.PatientFile DROP COLUMN	FileDate
ALTER TABLE dbo.PatientFile DROP COLUMN	ReceiveDate

ALTER TABLE dbo.PatientFile ADD CONSTRAINT
	FK_PatientFile_PatientFileGroup FOREIGN KEY
	(
	FileGroupId
	) REFERENCES dbo.PatientFileGroup
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
GO

ALTER TABLE dbo.PatientFileGroup ADD
	CreatedOn datetime NOT NULL
GO




ALTER TABLE dbo.[File] ADD
	RelatedFileId int NULL,
	RelationType nvarchar(50) NULL
GO

ALTER TABLE dbo.[File] ADD CONSTRAINT
	FK_File_File FOREIGN KEY
	(
	RelatedFileId
	) REFERENCES dbo.[File]
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

ALTER TABLE dbo.[File] ADD
	ExpirationDate datetime NULL
GO
CREATE NONCLUSTERED INDEX IX_File_ExpirationDate ON dbo.[File]
	(
	ExpirationDate
	)
GO

EXEC sp_rename 'File', 'FileMetadata'
EXEC sp_rename 'FileMetadata.RelatedFileId', 'RelatedFileMetadataId', 'COLUMN'
EXEC sp_rename 'FileMetadata.IX_File_ExpirationDate', 'IX_FileMetadata_ExpirationDate', 'INDEX'
EXEC sp_rename 'FileMetadata.PK_File', 'PK_FileMetadata', 'INDEX'
EXEC sp_rename 'FK_File_File', 'FK_FileMetadata_FileMetadata'
EXEC sp_rename 'FK_PatientFile_File', 'FK_PatientFile_FileMetadata'
EXEC sp_rename 'PatientFile.FileId', 'FileMetadataId', 'COLUMN'

EXEC sp_rename 'FileMetadata.FileName', 'BlobName', 'COLUMN'

ALTER TABLE dbo.FileMetadata ADD
	SourceFileName nvarchar(250) NULL
GO
update dbo.FileMetadata set SourceFileName = BlobName
GO
ALTER TABLE dbo.FileMetadata ALTER COLUMN
	SourceFileName nvarchar(250) NOT NULL
GO
ALTER TABLE dbo.FileMetadata ALTER COLUMN
	BlobName nvarchar(500) NOT NULL
GO
ALTER TABLE dbo.FileMetadata ADD
	Tag nvarchar(64) NULL
GO
CREATE NONCLUSTERED INDEX IX_FileMetadata_ContainerName_Tag ON dbo.FileMetadata
	(
	ContainerName,
	Tag
	)
GO




ALTER TABLE dbo.FileMetadata ADD
	OwnerUserId int NULL
ALTER TABLE dbo.FileMetadata ADD CONSTRAINT
	FK_FileMetadata_OwnerUser FOREIGN KEY
	(
	OwnerUserId
	) REFERENCES dbo.[User]
	(
	Id
	) ON UPDATE  NO ACTION
	 ON DELETE  NO ACTION
GO



COMMIT
GO
