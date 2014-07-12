-- Выполняется перед применением каждого пакета обновления, включенного в дистрибутив, в процессе обновления БД. Для сравнения см. PreUpdate.sql.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DropEntityFieldConstraintsForReferenceLinks]') AND type in (N'P', N'PC')) 
BEGIN
	DECLARE @do nvarchar(1000)
	SET @do	 = 'EXEC[dbo].[DropEntityFieldConstraintsForReferenceLinks]'
	EXEC sp_executesql @do
END;


ALTER INDEX [Unique_idParentNull] ON ref.Filter DISABLE;
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'Unique_isDescription')
ALTER INDEX [Unique_isDescription] ON ref.EntityField DISABLE;
IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'Unique_isCaption')
ALTER INDEX [Unique_isCaption] ON ref.EntityField DISABLE;
