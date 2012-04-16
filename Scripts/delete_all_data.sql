EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

EXEC sp_MSForEachTable '
 IF OBJECTPROPERTY(object_id(''?''), ''TableHasForeignRef'') = 1
  DELETE FROM ?
 else 
  TRUNCATE TABLE ?
'
GO


EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
GO


EXEC sp_MSForEachTable ' 
IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 
DBCC CHECKIDENT (''?'', RESEED, 0) 
' 
GO
