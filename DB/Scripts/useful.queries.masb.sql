-- Last used practices
select PR.Id, PR.UrlIdentifier, LastActiveOn, U.UserName from practice PR
 join [user] U on PR.OwnerId = U.Id
 order by LastActiveOn desc

--insert into glb_token (Value, ExpirationDate, [Type], Name) values ('5a740aca6e1046b7904b4df6d5fa6826', '2013-05-10', 'VerifyPracticeAndEmail', 'Practice=evandrofontesdeoliveira&UserName=evafoli')

-- All tokens
select * from glb_token

-- Info about an user
select U.Id, U.UserName, U.SYS_PasswordAlt, PR.UrlIdentifier, PR.Id from [user] U join Practice PR on PR.OwnerId = U.Id
	where UserName = 'Alefarpaiva'

--update [user] set SYS_PasswordAlt = 'Masb@1567' where Id = 29

--delete from glb_token where Id = 21

select * from Person 
join [user] U on U.PersonId = Person.Id
where U.PracticeId = 22

select OwnerId, SYS_PasswordAlt, UserName, [Password] from practice PR join [user] U on PR.OwnerId = U.Id

--update [user] set SYS_PasswordAlt = 'Masb@1567' where Id = 1

select UserName from [user]

select * from Patient








-- check datetime columns of medical record items
select CreatedOn, MedicalRecordDate from dbo.Anamnese						where PracticeId = 22
select CreatedOn, MedicalRecordDate from dbo.Diagnosis						where PracticeId = 22
select CreatedOn, MedicalRecordDate from dbo.DiagnosticHypothesis			where PracticeId = 22
select CreatedOn, RequestDate from dbo.ExaminationRequest					where PracticeId = 22
select CreatedOn, ExaminationDate, ReceiveDate from dbo.ExaminationResult	where PracticeId = 22
select CreatedOn, IssuanceDate from dbo.MedicalCertificate					where PracticeId = 22
select FileDate, ReceiveDate from dbo.PatientFile							where PracticeId = 22
select CreatedOn, MedicalRecordDate from dbo.PhysicalExamination			where PracticeId = 22
select CreatedOn, IssuanceDate from dbo.Receipt								where PracticeId = 22
