DECLARE @DataBaseName NVARCHAR(100)
SET @DataBaseName = DB_NAME()+'_Audit'
DECLARE @query NVARCHAR (MAX)

if EXISTS (SELECT * FROM sys.databases where name = @DataBaseName)
BEGIN
	SET @query = 'USE [' + @DataBaseName + ']

	IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N''[dbo].[FullReportView]''))
	DROP VIEW [dbo].[FullReportView]'
	EXEC (@query)

	SET @query = 'USE [' + @DataBaseName + '];
	EXEC(''
	CREATE VIEW [dbo].[FullReportView] AS
					SELECT 
						D.id, 
						D.[Before], 
						D.[After], 
						E.Caption as EntityCaption, 
						D.EntityId, 
						D.Operation, 
						D.ElementId, 
						D.IdUser,
						U.Caption as UserCaption, 
						([' + DB_NAME() + '].[dbo].[GetCaption]( D.EntityId, D.ElementId ) ) as ElementCaption, 
						D.[Date] 
					FROM 
						[' + @DataBaseName + '].dbo.data D 
						LEFT JOIN [' + DB_NAME() + '].[ref].[Entity] E on E.id = D.EntityId
						LEFT JOIN [' + DB_NAME() + '].[ref].[User] U on U.id = D.IdUser
		  '')                    
	'
	EXEC (@query)
end