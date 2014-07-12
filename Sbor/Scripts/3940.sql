IF NOT EXISTS (SELECT * FROM enm.SourcesDataReports WHERE id = 0)
INSERT INTO [enm].[SourcesDataReports] (id,	Name,	Caption,	[Description])
Values(0,	'BudgetEstimates'	,'Системная',	NULL);

IF NOT EXISTS (SELECT * FROM enm.SourcesDataReports WHERE id = 1)
INSERT INTO [enm].[SourcesDataReports] (id,	Name,	Caption,	[Description])
Values(1	,'Document',	'Предустановленная',	NULL);

IF NOT EXISTS (SELECT * FROM enm.SourcesDataReports WHERE id = 2)
INSERT INTO [enm].[SourcesDataReports] (id,	Name,	Caption,	[Description])
Values(2	,'JustificationBudget'	,'Пользовательская'	,NULL);

IF NOT EXISTS (SELECT * FROM enm.SourcesDataReports WHERE id = 4)
INSERT INTO [enm].[SourcesDataReports] (id,	Name,	Caption,	[Description])
Values(4	,'ResourceMaintenanceActivities',	'Предустановленная',	NULL);

IF NOT EXISTS (SELECT * FROM enm.SourcesDataReports WHERE id = 5)
INSERT INTO [enm].[SourcesDataReports] (id,	Name,	Caption,	[Description])
Values(5,	'RefSystemGoal',	'Пользовательская',	NULL);
