IF OBJECT_ID (N'dbo.aaa', N'U') IS NOT NULL
    DROP TABLE dbo.aaa;
select IDENTITY(int, 1,1) as id,
'DELETE a FROM ['+ 
		case	when b.idEntityType=1 then 'enm'
		when b.idEntityType=3 then 'ref'
		when b.idEntityType=4 then 'tp'
		when b.idEntityType=5 then 'ml'
		when b.idEntityType=6 then 'doc'
		when b.idEntityType=7 then 'tool'
		when b.idEntityType=8 then 'reg'
		when b.idEntityType=9 then 'rep' END
		+'].['+b.Name+'] a LEFT OUTER JOIN ['+
		case	when c.idEntityType=1 then 'enm'
		when c.idEntityType=3 then 'ref'
		when c.idEntityType=4 then 'tp'
		when c.idEntityType=5 then 'ml'
		when c.idEntityType=6 then 'doc'
		when c.idEntityType=7 then 'tool'
		when c.idEntityType=8 then 'reg'
		when c.idEntityType=9 then 'rep' END
		+'].['+c.Name+'] b ON '+' a.['+a.Name+']=b.[id] WHERE a.['+a.Name+'] IS NOT NULL AND b.[id] IS NULL ' +
		case when (EXISTS(SELECT * FROM ref.EntityField WHERE [idEntity] = b.id AND NAME = 'idTerminator')) then 'AND a.idTerminator is not null' ELSE '' end 
		
		as cmd, 0 as [count] 
INTO aaa
from ref.EntityField a
	inner join ref.Entity b on b.id=a.[idEntity]
	inner join ref.Entity c on c.id=a.idEntityLink
where a.idForeignKeyType is null and a.idEntityFieldType=7 and a.idCalculatedFieldType is NULL AND b.idEntityType = 8

declare @id int, @cmd nvarchar(1000), @count int
declare ttt cursor for select id, cmd, [count] from aaa
open ttt

FETCH NEXT FROM ttt into @id, @cmd, @count
while @@FETCH_STATUS=0
begin
	exec(@cmd)
FETCH NEXT FROM ttt into @id, @cmd, @count
end

close ttt
deallocate ttt