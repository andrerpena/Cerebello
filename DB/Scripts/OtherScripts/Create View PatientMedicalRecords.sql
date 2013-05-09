DROP VIEW [dbo].[PatientMedicalRecords]
GO

CREATE VIEW [dbo].[PatientMedicalRecords]
AS
    SELECT     'Anamnese' as [Type], PracticeId,PatientId, CreatedOn, MedicalRecordDate as [Date], NULL as ReceiveDate
    FROM         Anamnese
    UNION
    SELECT     'Diagnosis' as [Type], PracticeId,PatientId,CreatedOn, MedicalRecordDate as [Date], NULL as ReceiveDate
    FROM         Diagnosis
    UNION
    SELECT     'DiagnosticHypothesis' as [Type], PracticeId,PatientId,CreatedOn, MedicalRecordDate as [Date], NULL as ReceiveDate
    FROM         DiagnosticHypothesis
    UNION
    SELECT     'ExaminationRequest' as [Type], PracticeId,PatientId,CreatedOn, RequestDate as [Date], NULL as ReceiveDate
    FROM         ExaminationRequest
    UNION
    SELECT     'ExaminationResult' as [Type], PracticeId,PatientId,CreatedOn, ExaminationDate as [Date], ReceiveDate
    FROM         ExaminationResult
    UNION
    SELECT     'MedicalCertificate' as [Type], PracticeId,PatientId,CreatedOn, IssuanceDate as [Date], NULL as ReceiveDate
    FROM         MedicalCertificate
    UNION
    SELECT     'PatientFile' as [Type], PracticeId,PatientId,ReceiveDate AS CreatedOn, FileDate as [Date], ReceiveDate
    FROM         PatientFile
    UNION
    SELECT     'PhysicalExamination' as [Type], PracticeId,PatientId,CreatedOn, MedicalRecordDate as [Date], NULL as ReceiveDate
    FROM         PhysicalExamination
    UNION
    SELECT     'Receipt' as [Type], PracticeId,PatientId,CreatedOn, IssuanceDate as [Date], NULL as ReceiveDate
    FROM         Receipt

GO
