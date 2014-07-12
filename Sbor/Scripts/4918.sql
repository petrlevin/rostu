--SBORIII-1050 

-- удаляем все существующие ныне временные экземпляры отчетов
exec dbo.DelTempReports;

-- устанавливаем для ссылочных полей отчетов тип внешнего ключа "Без обеспечения ссылочной целостности"
update ref.EntityField set idForeignKeyType = 1
from 
	ref.EntityField
	inner join ref.Entity on ref.EntityField.[idEntity] = ref.Entity.id
where
	ref.Entity.idEntityType = 9
	and ref.EntityField.idEntityFieldType in (
		7,          -- ссылка
		20,21,22,23 -- общая ссылка
	)
	and ref.EntityField.idForeignKeyType <> 1