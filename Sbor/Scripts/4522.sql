IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE NAME LIKE N'%DEFAULT_TemplateExport_idSelectionType%' AND type = 'D')
BEGIN
ALTER TABLE [ref].[TemplateExport] DROP CONSTRAINT [DEFAULT_TemplateExport_idSelectionType]
END;

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE NAME LIKE N'%DEFAULT_TemplateExport_idLinkedSelectionType%' AND type = 'D')
BEGIN
ALTER TABLE [ref].[TemplateExport] DROP CONSTRAINT [DEFAULT_TemplateExport_idLinkedSelectionType]
END;

UPDATE ref.EntityField
SET Name = 'Caption'
WHERE id = -1744828228;


UPDATE ref.EntityField
SET Name = 'idSelectionType'
WHERE id = -1744828227;