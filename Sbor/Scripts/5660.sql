;
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'idActivityType' AND [object_id] = OBJECT_ID('tp.PublicInstitutionEstimate_Activity'))
DROP INDEX [idActivityType] ON [tp].[PublicInstitutionEstimate_Activity]
;
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_idActivityType_PublicInstitutionEstimate_Activity_ActivityType')
ALTER TABLE [tp].[PublicInstitutionEstimate_Activity] DROP CONSTRAINT [FK_idActivityType_PublicInstitutionEstimate_Activity_ActivityType]
;
if exists(select * from sys.columns where Name = N'idActivityType' and Object_ID = Object_ID(N'PublicInstitutionEstimate_Activity')) 
ALTER TABLE [tp].[PublicInstitutionEstimate_Activity] DROP COLUMN [idActivityType]
;