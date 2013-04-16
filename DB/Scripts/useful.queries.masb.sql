-- Last used practices
select PR.Id, PR.UrlIdentifier, LastActiveOn, U.UserName from practice PR
 join [user] U on PR.OwnerId = U.Id
 order by LastActiveOn desc

--insert into glb_token (Value, ExpirationDate, [Type], Name) values ('5a740aca6e1046b7904b4df6d5fa6826', '2013-05-10', 'VerifyPracticeAndEmail', 'Practice=evandrofontesdeoliveira&UserName=evafoli')

-- All tokens
select * from glb_token

-- Info about an user
select U.Id, U.UserName, U.SYS_PasswordAlt, PR.UrlIdentifier from [user] U join Practice PR on PR.OwnerId = U.Id
	where UserName = 'Alefarpaiva'

--update [user] set SYS_PasswordAlt = 'Masb@1567' where Id = 29

--delete from glb_token where Id = 21

select * from Person 
join [user] U on U.PersonId = Person.Id
where U.PracticeId = 22

select OwnerId, SYS_PasswordAlt, UserName, Password from practice PR join [user] U on PR.OwnerId = U.Id

--update [user] set SYS_PasswordAlt = 'Masb@1567' where Id = 1

select UserName from [user]

select * from Patient








-- check datetime columns of medical record items
select MedicalRecordDate from dbo.Anamnese
select MedicalRecordDate from dbo.Diagnosis
select MedicalRecordDate from dbo.DiagnosticHypothesis
select RequestDate from dbo.ExaminationRequest
select ExaminationDate, ReceiveDate from dbo.ExaminationResult
select IssuanceDate from dbo.MedicalCertificate
select FileDate, ReceiveDate from dbo.PatientFile
select MedicalRecordDate from dbo.PhysicalExamination
select IssuanceDate from dbo.Receipt
