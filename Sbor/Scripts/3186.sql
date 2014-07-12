
declare @cursor int
declare @sqlStr nvarchar(max)

set @sqlStr = '
select 
	case e.idEntityType  when 6 then ''doc'' when 4 then ''tp'' else ''ref'' end +''.''+ e.Name  as EntityName,
	ef.Name as FieldName,
	ef.AllowNull,
	case el.idEntityType when 6 then ''doc'' when 4 then ''tp'' else ''ref'' end +''.''+ el.Name as LinkName
from 
	ref.Entity as e
	inner join ref.EntityField as ef on ef.[idEntity] = e.id
	inner join ref.Entity as el on el.id = ef.idEntityLink
where 
	e.Name like ''ActivityOfSBP%''
	AND ef.idEntityFieldType = 7 and ef.idForeignKeyType is null and ef.idCalculatedFieldType is null
'

declare @EntityName nvarchar(max)
declare @FieldName nvarchar(max)
declare @AllowNull bit
declare @LinkName nvarchar(max)


EXEC sp_cursoropen @cursor OUTPUT, @sqlStr, 2, 8193
-- переименовали курсор
EXEC sp_cursoroption @cursor, 2, 'objel'
		
FETCH NEXT FROM objel INTO @EntityName, @FieldName, @AllowNull, @LinkName

-- цикл по СБП
WHILE @@FETCH_STATUS = 0
    BEGIN
		declare @curSql nvarchar(max)
		declare @count int

		set @curSql = 'select @count = count(*) from '+@EntityName+' where '+@FieldName+' not in (select id from '+@LinkName+')'
		EXEC sp_executesql @curSql,
			N'@count int OUTPUT',  
			@count = @count OUTPUT
		
		IF (@count > 0)
		BEGIN
			
			--PRINT @count
		
			IF (@AllowNull = 1)
				BEGIN
					set @curSql = 'update '+@EntityName+' set '+@FieldName+'=null where '+@FieldName+' not in (select id from '+@LinkName+')'
				END
			ELSE
				BEGIN
					set @curSql = 'delete '+@EntityName+' where '+@FieldName+' not in (select id from '+@LinkName+')'
				END
			
			EXEC sp_executeSql @curSql
			PRINT @curSql
			
		END
		
		FETCH NEXT FROM objel INTO @EntityName, @FieldName, @AllowNull, @LinkName
	END	

CLOSE objel
DEALLOCATE objel
