-- Выполняется ОДИН раз после обновлении БД из дистрибутива. Для сравнения см. PostUpdateDb.sql.
declare @str varchar(max);
SET @str=(SELECT 'EXECUTE [dbo].[CreateOrAlterIndex] '+CAST(id as varchar)+'; EXECUTE [dbo].[CreateOrAlterVersioningTrigger] '+CAST(id as varchar)+';' AS [text()] FROM [ref].[Index] WHERE [idRefStatus]=2 FOR XML PATH (''));
exec(@str);
SET @str=(SELECT 'EXEC [dbo].[CreateOrAlterReferencedGenericLinksTrigger] '+CAST(id as varchar)+';' AS [text()] FROM [ref].[Entity] WHERE AllowGenericLinks=1 FOR XML PATH (''))
exec(@str);
SET @str=(SELECT 'EXEC [dbo].[CreateOrAlterReferencesGenericLinksTrigger] '+CAST(a.id as varchar)+';' AS [text()] from ref.Entity a WHERE EXISTS(SELECT 1 FROM ref.EntityField WHERE [idEntity]=a.id AND idEntityFieldType in (20,21,22,23)) FOR XML PATH (''))
exec(@str);
SET @str=(SELECT 'EXECUTE [dbo].[CreateOrAlterUserDefineTableTypeByIdEntity] '+CAST(id as varchar)+';' AS [text()] FROM [ref].[Entity] WHERE [idEntityType]<>1 FOR XML PATH (''));
exec(@str);

if EXISTS(select * from sys.messages where message_id=50001)
	EXEC sp_dropmessage @msgnum=50001, @lang = 'us_english';
EXEC sp_addmessage @msgnum=50001, @severity=16, @msgtext=N'Error insert record in versioning Entity. Unique index "%s"', @lang = 'us_english';

if EXISTS(select * from sys.messages where message_id=50002)
	EXEC sp_dropmessage @msgnum=50002, @lang = 'us_english'
EXEC sp_addmessage @msgnum=50002, @severity=16, @msgtext=N'Error in versioning tigger. ValidityFrom>ValidityTo', @lang = 'us_english';

if EXISTS(select * from sys.messages where message_id=50003)
	EXEC sp_dropmessage @msgnum=50003, @lang = 'us_english'
EXEC sp_addmessage @msgnum=50003, @severity=16, @msgtext=N'Not set Entity for GenericLink', @lang = 'us_english';

if EXISTS(select * from sys.messages where message_id=50004)
	EXEC sp_dropmessage @msgnum=50004, @lang = 'us_english'
EXEC sp_addmessage @msgnum=50004, @severity=16, @msgtext=N'Conflicted with constraint for GenericLink', @lang = 'us_english';

if EXISTS(select * from sys.messages where message_id=50005)
	EXEC sp_dropmessage @msgnum=50005, @lang = 'us_english'
EXEC sp_addmessage @msgnum=50005, @severity=16, @msgtext=N'The cycle in the hierarchy is not valid', @lang = 'us_english';

exec  [dbo].[CreateOrAlterAllowGenericLinks]
exec  [dbo].[CreateOrEnableEntityFieldConstraintsForReferenceLinks]

ALTER INDEX [Unique_idParentNull] ON ref.Filter REBUILD;
/*
	Установить права на SELECT/EXECUTE для всех скалярных и табличных функций для заданного пользователя
*/
declare @userName nvarchar(50) = 'public'; -- Имя роли или пользователя, котору требуется установить права на программируемые объекты
declare @grant_OR_revoke bit = 1; -- Установить/снять права (1 = grant, 0 = revoke)


/*********************************************************************/
declare @functionName nvarchar(200);
declare @functionType varchar(5);
declare @sql nvarchar(600);

declare function_cursor cursor for 
select 
	'[' + sch.name + '].[' + obj.[name] + ']',
	obj.[type]
from sys.objects as obj
	inner join sys.schemas as sch on obj.schema_id=sch.schema_id
where type_desc LIKE '%FUNCTION%'

/*type 
	AF(AGGREGATE_FUNCTION)
	FN(SQL_SCALAR_FUNCTION)
	FS(CLR_SCALAR_FUNCTION)
	
	FT(CLR_TABLE_VALUED_FUNCTION)
	IF(SQL_INLINE_TABLE_VALUED_FUNCTION)
	
	P (SQL_STORED_PROCEDURE)
	PC(CLR_STORED_PROCEDURE)
*/

open function_cursor;

fetch next from function_cursor into @functionName, @functionType;

while @@fetch_status = 0
begin
	
	--SCALAR_FUNCTION and AGGREGATE_FUNCTION
	if ( @functionType in ('AF', 'FS', 'FN') )
	begin
		select 
			@sql = (case @grant_OR_revoke when 1 then 'grant' else 'revoke' end) + ' execute on ' + @functionName + ' to [' + @userName +']';
			exec (@sql);
	end
	
	--TABLE_VALUED_FUNCTION
	if (@functionType in ('FT', 'IF'))
	begin
		select 
			@sql = (case @grant_OR_revoke when 1 then 'grant' else 'revoke' end) + ' select on ' + @functionName + ' to [' + @userName + ']';
			exec (@sql);
	end
	
	/*if (@functionType in ('P','PP'))
	begin
	end*/

	fetch next from function_cursor into @functionName, @functionType;
end

close function_cursor;
deallocate function_cursor;

set arithabort on;

DECLARE @DB varchar(50)
SELECT @DB = DB_NAME()

DECLARE @SQLString VARCHAR(200)
SET @SQLString = 'ALTER DATABASE ' + @DB + ' SET ALLOW_SNAPSHOT_ISOLATION ON;'
EXEC(@SqlString);
SET @SQLString = 'ALTER DATABASE ' + @DB + ' SET READ_COMMITTED_SNAPSHOT ON;'
EXEC(@SqlString);


/*затычка для обновления с 3.0.3.2 на trunk*/
ALTER TABLE rep.SystemOfActivitySBP
	CHECK CONSTRAINT FK_idSourcesDataReports_SystemOfActivitySBP_SourcesDataReports

ALTER TABLE rep.DirectionAndFundingOfDepartment
	CHECK CONSTRAINT FK_idSourcesDataReports_DirectionAndFundingOfDepartment_SourcesDataReports

ALTER TABLE rep.RegistryGoal
	CHECK CONSTRAINT FK_DisplayResourceSupport_RegistryGoal_SourcesDataReports
	
ALTER TABLE rep.PassportStateProgram
	CHECK CONSTRAINT FK_idSourcesDataReports_PassportStateProgram_SourcesDataReports
/*затычка для обновления с 3.0.3.2 на trunk*/