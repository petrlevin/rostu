CREATE TABLE [dbo].[GenericLinks](
	[idReferenced] [int] NOT NULL,
	[idReferencedEntity] [int] NOT NULL,
	[idReferences] [int] NOT NULL,
	[idReferencesEntity] [int] NOT NULL,
	[idReferencesEntityField] [int] NOT NULL,
 CONSTRAINT [PK_dbo.GenericLinks] PRIMARY KEY CLUSTERED 
(
	[idReferenced] ASC,
	[idReferencedEntity] ASC,
	[idReferences] ASC,
	[idReferencesEntity] ASC,
	[idReferencesEntityField] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
);

CREATE NONCLUSTERED INDEX [IX_GenericLinks] ON [dbo].[GenericLinks] 
(
	[idReferenced] ASC,
	[idReferencedEntity] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON);

