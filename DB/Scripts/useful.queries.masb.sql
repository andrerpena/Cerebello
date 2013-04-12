select PR.Id, PR.UrlIdentifier, LastActiveOn from practice PR
 join [user] U on PR.OwnerId = U.Id
 order by LastActiveOn desc

--insert into glb_token (Value, ExpirationDate, [Type], Name) values ('5a740aca6e1046b7904b4df6d5fa6826', '2013-05-10', 'VerifyPracticeAndEmail', 'Practice=evandrofontesdeoliveira&UserName=evafoli')

select * from glb_token

select Id, UserName, SYS_PasswordAlt from [user] where UserName = 'evafoli'

--update [user] set SYS_PasswordAlt = 'Masb@1567' where Id = 16

--delete from glb_token where Id = 21

select * from Person 
join [user] U on U.PersonId = Person.Id
where U.PracticeId = 22

select OwnerId, SYS_PasswordAlt, UserName, Password from practice PR join [user] U on PR.OwnerId = U.Id

--update [user] set SYS_PasswordAlt = 'Masb@1567' where Id = 1

select UserName from [user]
