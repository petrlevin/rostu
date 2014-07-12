DISABLE TRIGGER [ref].[EntityIUD] ON ref.Entity
GO
DISABLE TRIGGER [ref].[EntityFieldIUD] ON ref.EntityField
GO

update ref.EntityField set idFieldDefaultValueType = 1
from 
	ref.EntityField
	inner join ref.Entity on ref.EntityField.[idEntity] = ref.Entity.id
where 
	ref.Entity.Name in ('Entity', 'EntityField')
	and ref.EntityField.DefaultValue <> ''
	and ref.EntityField.DefaultValue is not null
	and idFieldDefaultValueType is null
GO

ENABLE TRIGGER [ref].[EntityIUD] ON ref.Entity
GO
ENABLE TRIGGER [ref].[EntityFieldIUD] ON ref.EntityField
GO
