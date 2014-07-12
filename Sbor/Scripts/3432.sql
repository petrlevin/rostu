IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GenericLinks]') AND name = N'idReferences')
DROP INDEX [idReferences] ON [dbo].[GenericLinks] WITH ( ONLINE = OFF )

CREATE NONCLUSTERED INDEX [idReferences] ON [dbo].[GenericLinks] ([idReferences], [idReferencesEntity],[idReferencesEntityField]) INCLUDE ([idReferenced],[idReferencedEntity])