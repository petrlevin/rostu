ALTER TABLE [tp].[StateProgram_ListSubProgram] 
DROP CONSTRAINT [FK_idDocumentEntity_StateProgram_ListSubProgram_Entity]

IF  EXISTS (SELECT * FROM sys.indexes 
WHERE object_id = OBJECT_ID(N'[tp].[StateProgram_ListSubProgram]') 
	AND name = N'idDocumentEntity')
DROP INDEX [idDocumentEntity] ON [tp].[StateProgram_ListSubProgram] WITH ( ONLINE = OFF )
GO

ALTER TABLE [tp].[StateProgram_ListSubProgram] 
DROP COLUMN [idDocumentEntity]

ALTER TABLE [tp].[StateProgram_DepartmentGoalProgramAndKeyActivity]
DROP CONSTRAINT [FK_idDocumentEntity_StateProgram_DepartmentGoalProgramAndKeyActivity_Entity]

IF  EXISTS (SELECT * FROM sys.indexes 
WHERE object_id = OBJECT_ID(N'[tp].[StateProgram_DepartmentGoalProgramAndKeyActivity]') 
	AND name = N'idDocumentEntity')
DROP INDEX [idDocumentEntity] ON [tp].[StateProgram_DepartmentGoalProgramAndKeyActivity] WITH ( ONLINE = OFF )
GO

ALTER TABLE [tp].[StateProgram_DepartmentGoalProgramAndKeyActivity]
DROP COLUMN [idDocumentEntity]
