/**
 * Скрипт для разворачивания платформенной БД.
 */


 /**********************************************************************************************************************
  * Таблицы
  */

/** 
 * Entity
 */
CREATE TABLE [ref].[Entity]
(
	[id] INT NOT NULL IDENTITY, 
	[tstamp] TIMESTAMP NOT NULL, 
	[Name] NVARCHAR(100) NOT NULL, 
	[Caption] NVARCHAR(400) NOT NULL, 
	[Description] NVARCHAR(MAX) NULL, 
	[idEntityType] TINYINT NOT NULL,
	[idProject] INT NULL,
	[isSystem] BIT NULL DEFAULT 0,
	[Order] INT NULL,
	[Ordered] BIT NULL DEFAULT 0,
	[AllowAttachments] BIT NULL DEFAULT 0,
	[isVersioning] BIT NOT NULL DEFAULT 0,
	[GenerateEntityClass] BIT NOT NULL DEFAULT 0,
	[idEntityGroup] INT NULL,
	[AllowGenericLinks] BIT NOT NULL DEFAULT 0,
	[AllowLinks] BIT DEFAULT 1
);

ALTER TABLE [ref].[Entity] WITH CHECK ADD CONSTRAINT
	[PK_Entity] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
CREATE NONCLUSTERED INDEX [idEntityType] ON [ref].[Entity] ([idEntityType]);
CREATE NONCLUSTERED INDEX [idProject] ON [ref].[Entity] ([idProject]);

CREATE TYPE [gen].[Entity] AS TABLE
(
	[id] INT NOT NULL, 
	[Name] NVARCHAR(100) NOT NULL, 
	[Caption] NVARCHAR(400) NOT NULL, 
	[Description] NVARCHAR(MAX) NULL, 
	[idEntityType] TINYINT NOT NULL,
	[idProject] INT NULL,
	[isSystem] BIT NULL DEFAULT 0,
	[Order] INT NULL,
	[Ordered] BIT NULL DEFAULT 0,
	[AllowAttachments] BIT NULL DEFAULT 0,
	[isVersioning] BIT NOT NULL DEFAULT 0,
	[GenerateEntityClass] BIT NOT NULL DEFAULT 0,
	[idEntityGroup] INT NULL,
	[AllowGenericLinks] BIT DEFAULT 0,
	[AllowLinks] BIT DEFAULT 1
);

/** 
 * EntityField
 */
CREATE TABLE [ref].[EntityField]
(
	[id] INT NOT NULL IDENTITY, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(100) NOT NULL, 
	[Caption] NVARCHAR(400) NOT NULL, 
	[Description] NVARCHAR(MAX) NULL, 
	[idEntity] INT NOT NULL, 
	[idEntityFieldType] TINYINT NOT NULL, 
	[idCalculatedFieldType] TINYINT NULL, 
	[Size] SMALLINT NULL, 
	[Precision] TINYINT NULL,
	[Expression] NVARCHAR(200) NULL, 
	[idEntityLink] INT NULL, 
	[idOwnerField] INT NULL,
	[idForeignKeyType] TINYINT NULL, 
	[AllowNull] BIT NOT NULL DEFAULT 1, 
	[isCaption] BIT NULL DEFAULT 0, 
	[isDescription] BIT NULL DEFAULT 0, 
	[DefaultValue] NVARCHAR(MAX) NULL,
	[idFieldDefaultValueType] TINYINT NULL,
	[isSystem] BIT NULL DEFAULT 0,
	[ReadOnly] BIT NULL DEFAULT 0,
	[isHidden] BIT NOT NULL DEFAULT 0,
	[RegExpValidator] NVARCHAR(600) NULL,
	[Tooltip] NVARCHAR(MAX) NULL
);

ALTER TABLE [ref].[EntityField] WITH CHECK ADD CONSTRAINT
	[PK_EntityField] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

CREATE UNIQUE NONCLUSTERED INDEX [Unique_isCaption] ON [ref].[EntityField] 
(
	[idEntity] ASC
)
WHERE ([isCaption]=(1))
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = ON, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON);

CREATE UNIQUE NONCLUSTERED INDEX [Unique_isDescription] ON [ref].[EntityField] 
(
	[idEntity] ASC
)
WHERE ([isDescription]=(1))
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = ON, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON);

CREATE NONCLUSTERED INDEX [idEntity] ON [ref].[EntityField] ([idEntity]);
CREATE NONCLUSTERED INDEX [idEntityFieldType] ON [ref].[EntityField] ([idEntityFieldType]);
CREATE NONCLUSTERED INDEX [idEntityLink] ON [ref].[EntityField] ([idEntityLink]) WHERE ([idEntityLink] IS NOT NULL);

CREATE TYPE [gen].[EntityField] AS TABLE
(
	[id] INT NOT NULL, 
	[Name] NVARCHAR(100) NOT NULL, 
	[Caption] NVARCHAR(400) NOT NULL, 
	[Description] NVARCHAR(MAX) NULL, 
	[idEntity] INT NOT NULL, 
	[idEntityFieldType] TINYINT NOT NULL, 
	[idCalculatedFieldType] TINYINT NULL, 
	[Size] SMALLINT NULL, 
	[Precision] TINYINT NULL,
	[Expression] NVARCHAR(200) NULL, 
	[idEntityLink] INT NULL, 
	[idOwnerField] INT NULL,
	[idForeignKeyType] TINYINT NULL, 
	[AllowNull] BIT NOT NULL DEFAULT 1, 
	[isCaption] BIT NULL DEFAULT 0, 
	[isDescription] BIT NULL DEFAULT 0, 
	[DefaultValue] NVARCHAR(MAX) NULL,
	[idFieldDefaultValueType] TINYINT NULL,
	[isSystem] BIT NULL DEFAULT 0,
	[ReadOnly] BIT NULL DEFAULT 0,
	[isHidden] BIT NOT NULL DEFAULT 0,
	[RegExpValidator] NVARCHAR(600) NULL,
	[Tooltip] NVARCHAR(MAX) NULL
);
/** 
 * EntityType
 */
CREATE TABLE [enm].[EntityType]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[EntityType] WITH CHECK ADD CONSTRAINT
	[PK_EntityType] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**
 * EntityFieldType
 */
CREATE TABLE [enm].[EntityFieldType]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[EntityFieldType] WITH CHECK ADD CONSTRAINT
	[PK_EntityFieldType] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**
 * IndexType
 */
CREATE TABLE [enm].[IndexType]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[IndexType] WITH CHECK ADD CONSTRAINT
	[PK_IndexType] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**
 * SolutionProject
 */
CREATE TABLE [enm].[SolutionProject]
(
	[id] INT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL,
);

ALTER TABLE [enm].[SolutionProject] WITH CHECK ADD CONSTRAINT
	[PK_SolutionProject] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**
 * FieldDefaultValueType
 */
CREATE TABLE [enm].[FieldDefaultValueType]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[FieldDefaultValueType] WITH CHECK ADD CONSTRAINT
	[PK_FieldDefaultValueType] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);


/**
 * Programmability
 */
CREATE TABLE [ref].[Programmability]
	(
	[id] int NOT NULL IDENTITY,
	[tstamp] timestamp NOT NULL,
	[Schema] nvarchar(50) NOT NULL,
	[Name] nvarchar(100) NOT NULL,
	[CreateCommand] nvarchar(MAX) NOT NULL,
	[idProgrammabilityType] tinyint NOT NULL,
	[Order] nvarchar(5) NULL,
	[idProject] int NULL,
	[isDisabled] bit NOT NULL
);

ALTER TABLE [ref].[Programmability] WITH CHECK ADD CONSTRAINT
	[PK_Programmability] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

ALTER TABLE [ref].[Programmability] ADD  CONSTRAINT [DEFAULT_Programmability_isDisabled]  DEFAULT ((0)) FOR [isDisabled];

/**
 * Filter
 */
CREATE TABLE [ref].[Filter]
	(
	[id] int NOT NULL IDENTITY,
	[Disabled] bit DEFAULT 0 ,
	[tstamp] timestamp NOT NULL,
	[idEntityField] int NOT NULL,
	[idLogicOperator] tinyint NOT NULL,
	[Not] bit NOT NULL DEFAULT(0),
	[idLeftEntityField] int NULL,
	[idComparisionOperator] tinyint NULL,
	[RightValue] nvarchar(256) NULL,
	[idRightEntityField] int NULL,
	[RightSqlExpression] nvarchar(MAX) NULL,
	[idParent] int NULL,
	[Description] nvarchar(MAX) NULL,
	[WithParents] bit NOT NULL DEFAULT(0)
);

ALTER TABLE [ref].[Filter] WITH CHECK ADD CONSTRAINT
	[PK_Filter] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

CREATE UNIQUE NONCLUSTERED INDEX [Unique_idParentNull] ON [ref].[Filter] 
(
	[idEntityField] ASC
)
WHERE ([idParent] IS NULL AND [Disabled] = 0)
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = ON, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON);

/**
 * Index
 */
CREATE TABLE [ref].[Index](
	[tstamp] [timestamp] NOT NULL,
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idEntity] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Caption] [nvarchar](400) NOT NULL,
	[idIndexType] [tinyint] NOT NULL,
	[isClustered] [bit] NOT NULL,
	[Filter] [nvarchar](100) NULL,
	[idRefStatus] [tinyint] NOT NULL
 CONSTRAINT [PK_Index] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [ref].[Index]  WITH CHECK ADD  CONSTRAINT [FK_idEntity_Index_Entity] FOREIGN KEY([idEntity])
REFERENCES [ref].[Entity] ([id])
ON DELETE CASCADE

ALTER TABLE [ref].[Index] CHECK CONSTRAINT [FK_idEntity_Index_Entity]

ALTER TABLE [ref].[Index]  WITH CHECK ADD  CONSTRAINT [FK_idIndexType_Index_IndexType] FOREIGN KEY([idIndexType])
REFERENCES [enm].[IndexType] ([id])

ALTER TABLE [ref].[Index] CHECK CONSTRAINT [FK_idIndexType_Index_IndexType]

ALTER TABLE [ref].[Index] ADD  CONSTRAINT [DEFAULT_Index_isClustered]  DEFAULT ((0)) FOR [isClustered]

/**
 * Index_EntityField
 */
CREATE TABLE [ml].[Index_EntityField_Indexable](
	[tstamp] [timestamp] NOT NULL,
	[idIndex] [int] NOT NULL,
	[idEntityField] [int] NOT NULL,
	[idIndexOrder] [int] NOT NULL DEFAULT 1,
	[idEntityFieldOrder] [int] NOT NULL DEFAULT 1
) ON [PRIMARY]

ALTER TABLE [ml].[Index_EntityField_Indexable] WITH CHECK ADD CONSTRAINT
	[PK_Index_EntityField_Indexable] PRIMARY KEY CLUSTERED 
	(
	[idIndex],
	[idEntityField]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

ALTER TABLE [ml].[Index_EntityField_Indexable]  WITH CHECK ADD  CONSTRAINT [FK_idEntityField_Index_EntityField_Indexable_EntityField] FOREIGN KEY([idEntityField])
REFERENCES [ref].[EntityField] ([id])

ALTER TABLE [ml].[Index_EntityField_Indexable] CHECK CONSTRAINT [FK_idEntityField_Index_EntityField_Indexable_EntityField]

ALTER TABLE [ml].[Index_EntityField_Indexable]  WITH CHECK ADD  CONSTRAINT [FK_idIndex_Index_EntityField_Indexable_Index] FOREIGN KEY([idIndex])
REFERENCES [ref].[Index] ([id])
ON DELETE CASCADE

ALTER TABLE [ml].[Index_EntityField_Indexable] CHECK CONSTRAINT [FK_idIndex_Index_EntityField_Indexable_Index]

----

CREATE TABLE [ml].[Index_EntityField_Included](
	[tstamp] [timestamp] NOT NULL,
	[idIndex] [int] NOT NULL,
	[idEntityField] [int] NOT NULL
) ON [PRIMARY]

ALTER TABLE [ml].[Index_EntityField_Included] WITH CHECK ADD CONSTRAINT
	[PK_Index_EntityField_Included] PRIMARY KEY CLUSTERED 
	(
	[idIndex],
	[idEntityField]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

ALTER TABLE [ml].[Index_EntityField_Included]  WITH CHECK ADD  CONSTRAINT [FK_idEntityField_Index_EntityField_Included_EntityField] FOREIGN KEY([idEntityField])
REFERENCES [ref].[EntityField] ([id])

ALTER TABLE [ml].[Index_EntityField_Included] CHECK CONSTRAINT [FK_idEntityField_Index_EntityField_Included_EntityField]

ALTER TABLE [ml].[Index_EntityField_Included]  WITH CHECK ADD  CONSTRAINT [FK_idIndex_Index_EntityField_Included_Index] FOREIGN KEY([idIndex])
REFERENCES [ref].[Index] ([id])
ON DELETE CASCADE

ALTER TABLE [ml].[Index_EntityField_Included] CHECK CONSTRAINT [FK_idIndex_Index_EntityField_Included_Index]

/**
 * ProgrammabilityType
 */
CREATE TABLE [enm].[ProgrammabilityType]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[ProgrammabilityType] WITH CHECK ADD CONSTRAINT
	[PK_ProgrammabilityType] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**********************************************************************************************************************
 * ForeignKeyType
**********************************************************************************************************************/
CREATE TABLE [enm].[ForeignKeyType]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[ForeignKeyType] WITH CHECK ADD CONSTRAINT
	[PK_ForeignKeyType] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**********************************************************************************************************************
 * CalculatedFieldType
**********************************************************************************************************************/
CREATE TABLE [enm].[CalculatedFieldType]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[CalculatedFieldType] WITH CHECK ADD CONSTRAINT
	[PK_CalculatedFieldType] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**********************************************************************************************************************
 * ComparisionOperator
**********************************************************************************************************************/
CREATE TABLE [enm].[ComparisionOperator]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[ComparisionOperator] WITH CHECK ADD CONSTRAINT
	[PK_ComparisionOperator] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**********************************************************************************************************************
 * LogicOperator
**********************************************************************************************************************/
CREATE TABLE [enm].[LogicOperator]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[LogicOperator] WITH CHECK ADD CONSTRAINT
	[PK_LogicOperator] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

/**********************************************************************************************************************
 * RefStatus
**********************************************************************************************************************/
CREATE TABLE [enm].[RefStatus]
(
	[id] TINYINT NOT NULL, 
	[tstamp] TIMESTAMP NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL, 
);

ALTER TABLE [enm].[RefStatus] WITH CHECK ADD CONSTRAINT
	[PK_RefStatus] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);


/**********************************************************************************************************************
 * EntityGroup
**********************************************************************************************************************/

CREATE TABLE [ref].[EntityGroup]
(
	[id] INT NOT NULL IDENTITY, 
	[idParent] INT NULL,
	[Caption] NVARCHAR(50) NOT NULL, 
	[Description] NVARCHAR(MAX) NULL, 
	[Order] INT NULL
);

ALTER TABLE [ref].[EntityGroup] WITH CHECK ADD CONSTRAINT
	[PK_EntityGroup] PRIMARY KEY CLUSTERED 
	(
	[id]
	) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);



/**********************************************************************************************************************
 * Типы
**********************************************************************************************************************/

CREATE TYPE [gen].[Enums] AS TABLE(
	[id] INT NOT NULL,
	[Name] NVARCHAR(50) NOT NULL, 
	[Caption] NVARCHAR(100) NULL, 
	[Description] NVARCHAR(400) NULL
);

/**********************************************************************************************************************
 * ItemsDependencies
**********************************************************************************************************************/
CREATE TABLE [reg].[ItemsDependencies](
	[tstamp] [timestamp] NOT NULL,
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idObject] [int] NOT NULL,
	[idObjectEntity] [int] NOT NULL,
	[idDependsOn] [int] NOT NULL,
	[idDependsOnEntity] [int] NOT NULL,
 CONSTRAINT [PK_DependingFieldsUDF] PRIMARY KEY CLUSTERED (
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [reg].[ItemsDependencies]  WITH CHECK ADD  CONSTRAINT [FK_idDependsOnEntity_DependingFieldsUDF_Entity] FOREIGN KEY([idDependsOnEntity])
REFERENCES [ref].[Entity] ([id])

ALTER TABLE [reg].[ItemsDependencies] CHECK CONSTRAINT [FK_idDependsOnEntity_DependingFieldsUDF_Entity]

ALTER TABLE [reg].[ItemsDependencies]  WITH CHECK ADD  CONSTRAINT [FK_idObjectEntity_DependingFieldsUDF_Entity] FOREIGN KEY([idObjectEntity])
REFERENCES [ref].[Entity] ([id])

ALTER TABLE [reg].[ItemsDependencies] CHECK CONSTRAINT [FK_idObjectEntity_DependingFieldsUDF_Entity]

/**********************************************************************************************************************
 * Общие ссылки
**********************************************************************************************************************/


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

CREATE NONCLUSTERED INDEX [idReferences] ON [dbo].[GenericLinks] ([idReferences], [idReferencesEntity],[idReferencesEntityField]) INCLUDE ([idReferenced],[idReferencedEntity])
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON);

CREATE TABLE [reg].[UpdateRevision](
	[tstamp] [timestamp] NOT NULL,
	[id] [int] IDENTITY(1,1) NOT NULL,
	[MajorVersionNumber] [int] NULL,
	[MinorVersionNumber] [int] NULL,
	[BuildNumber] [int] NULL,
	[Revision] [int] NULL,
	[ReleaseNumber] [int] NULL,
	[File] [varbinary](max) NULL,
	[Date] [datetime] NULL,
 CONSTRAINT [PK_UpdateRevisionUDF] PRIMARY KEY CLUSTERED (
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]