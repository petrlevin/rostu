UPDATE [ref].[Entity] 
SET AllowGenericLinks =0
WHERE AllowGenericLinks IS NULL

ALTER TABLE [ref].[Entity] ALTER COLUMN [AllowGenericLinks] BIT NOT NULL
