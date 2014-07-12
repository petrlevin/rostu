IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ref].[CalculationFormula]') AND name = N'idPublicLegalFormation')
DROP INDEX [idPublicLegalFormation] ON [ref].[CalculationFormula] WITH ( ONLINE = OFF );

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[ref].[CalculationFormula]') AND name = N'uniqCF')
DROP INDEX [uniqCF] ON [ref].[CalculationFormula] WITH ( ONLINE = OFF )

ALTER TABLE [ref].[CalculationFormula] ALTER COLUMN [idPublicLegalFormation] int NOT NULL;

CREATE NONCLUSTERED INDEX [idPublicLegalFormation] ON [ref].[CalculationFormula] 
(
	[idPublicLegalFormation] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE UNIQUE NONCLUSTERED INDEX [uniqCF] ON [ref].[CalculationFormula] 
(
	[idPublicLegalFormation] ASC,
	[Caption] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
