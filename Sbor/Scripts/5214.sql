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
