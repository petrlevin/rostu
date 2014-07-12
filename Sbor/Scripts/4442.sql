--удаляем индекс
IF EXISTS (SELECT name FROM sys.indexes
			WHERE object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_DistributionActivity]') and 
				name = 'idContingent') 
DROP INDEX [tp].[PublicInstitutionEstimate_DistributionActivity].[idContingent]
;

--удаляем ключ
IF  EXISTS (SELECT * FROM sys.foreign_keys 
				WHERE object_id = OBJECT_ID(N'[tp].[FK_idContingent_PublicInstitutionEstimate_DistributionActivity_Contingent]') AND 
				parent_object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_DistributionActivity]'))
ALTER TABLE [tp].[PublicInstitutionEstimate_DistributionActivity] 
DROP CONSTRAINT [FK_idContingent_PublicInstitutionEstimate_DistributionActivity_Contingent]
;


-- удаляем колонку
IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'PublicInstitutionEstimate_DistributionActivity'
           AND  COLUMN_NAME = 'idContingent')
           alter table tp.PublicInstitutionEstimate_DistributionActivity 
           drop column idContingent
;           
