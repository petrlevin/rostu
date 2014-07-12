;
IF EXISTS (SELECT name FROM sys.indexes
            WHERE name = N'Unique_isDescription')
    DROP INDEX Unique_isDescription ON ref.EntityField;
;
CREATE UNIQUE NONCLUSTERED INDEX [Unique_isDescription] ON [ref].[EntityField] 
(
	[idEntity] ASC
)
WHERE ([isDescription]=(1))
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = ON, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON);
;

update ref.EntityField
set isDescription = 0
where id = -2013265709