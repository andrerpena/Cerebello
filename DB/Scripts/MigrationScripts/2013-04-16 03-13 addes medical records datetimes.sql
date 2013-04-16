ALTER TABLE dbo.PatientFile ADD
	FileDate datetime NULL,
	ReceiveDate datetime NULL
go

UPDATE dbo.PatientFile set FileDate = (select CreatedOn from dbo.[File] F where F.Id = dbo.PatientFile.FileId),
	ReceiveDate = (select CreatedOn from dbo.[File] F where F.Id = dbo.PatientFile.FileId)
go

ALTER TABLE dbo.PatientFile ALTER COLUMN
	FileDate datetime NOT NULL
ALTER TABLE dbo.PatientFile ALTER COLUMN
	ReceiveDate datetime NOT NULL
go




ALTER TABLE dbo.Anamnese ADD
	MedicalRecordDate datetime NULL
go

UPDATE dbo.Anamnese set MedicalRecordDate = CreatedOn
go

ALTER TABLE dbo.Anamnese ALTER COLUMN
	MedicalRecordDate datetime NOT NULL
go




ALTER TABLE dbo.Diagnosis ADD
	MedicalRecordDate datetime NULL
go

UPDATE dbo.Diagnosis set MedicalRecordDate = CreatedOn
go

ALTER TABLE dbo.Diagnosis ALTER COLUMN
	MedicalRecordDate datetime NOT NULL
go




ALTER TABLE dbo.DiagnosticHypothesis ADD
	MedicalRecordDate datetime NULL
go

UPDATE dbo.DiagnosticHypothesis set MedicalRecordDate = CreatedOn
go

ALTER TABLE dbo.DiagnosticHypothesis ALTER COLUMN
	MedicalRecordDate datetime NOT NULL
go




ALTER TABLE dbo.ExaminationRequest ADD
	RequestDate datetime NULL
go

UPDATE dbo.ExaminationRequest set RequestDate = CreatedOn
go

ALTER TABLE dbo.ExaminationRequest ALTER COLUMN
	RequestDate datetime NOT NULL
go




ALTER TABLE dbo.ExaminationResult ADD
	ExaminationDate datetime NULL,
	ReceiveDate datetime NULL
go

UPDATE dbo.ExaminationResult set ExaminationDate = CreatedOn, ReceiveDate = CreatedOn
go

ALTER TABLE dbo.ExaminationResult ALTER COLUMN
	ExaminationDate datetime NOT NULL
ALTER TABLE dbo.ExaminationResult ALTER COLUMN
	ReceiveDate datetime NOT NULL
go




ALTER TABLE dbo.MedicalCertificate ADD
	IssuanceDate datetime NULL
go

UPDATE dbo.MedicalCertificate set IssuanceDate = CreatedOn
go

ALTER TABLE dbo.MedicalCertificate ALTER COLUMN
	IssuanceDate datetime NOT NULL
go




ALTER TABLE dbo.PhysicalExamination ADD
	MedicalRecordDate datetime NULL
go

UPDATE dbo.PhysicalExamination set MedicalRecordDate = CreatedOn
go

ALTER TABLE dbo.PhysicalExamination ALTER COLUMN
	MedicalRecordDate datetime NOT NULL
go




ALTER TABLE dbo.Receipt ADD
	IssuanceDate datetime NULL
go

UPDATE dbo.Receipt set IssuanceDate = CreatedOn
go

ALTER TABLE dbo.Receipt ALTER COLUMN
	IssuanceDate datetime NOT NULL
go




