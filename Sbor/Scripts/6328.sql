if exists(select * from sys.columns where Name = N'idAggregate' and Object_ID = Object_ID(N'ref.EntityFieldSetting'))
BEGIN
DECLARE @com NVARCHAR(MAX)
SELECT @com = '
delete from [ref].[EntityField] where [id]=-1744830375
insert into [enm].[Aggregate] ([id], [Name], [Caption], [Description]) select [id]+100, [Name], [Caption], [Description] from [enm].[Aggregate]
update ref.EntityFieldSetting set idAggregate=idAggregate+100
delete from [enm].[Aggregate] where id<100'
EXEC (@com)
END


GO
DISABLE TRIGGER [EntityFieldIUD] ON ref.EntityField
GO
DELETE FROM [ref].[EntityField] WHERE [id] IN (-1744830375)
GO
ENABLE TRIGGER [EntityFieldIUD] ON ref.EntityField