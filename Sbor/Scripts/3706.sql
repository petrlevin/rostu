DISABLE TRIGGER [ref].[EntityFieldIUD] ON [ref].[EntityField];
ALTER TABLE [ref].[EntityField] ADD [Tooltip] NVARCHAR(MAX) NULL;
SET IDENTITY_INSERT [ref].[EntityField] ON;
INSERT INTO [ref].[EntityField] ([id],[Name],[Caption],[idEntity],[idEntityFieldType],[AllowNull],[isHidden])
VALUES (-1207959219, 'Tooltip','Подсказка',-2147483614,17,1,0);
SET IDENTITY_INSERT [ref].[EntityField] OFF;
ENABLE TRIGGER [ref].[EntityFieldIUD] ON [ref].[EntityField];