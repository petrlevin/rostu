/*
DISABLE TRIGGER [ref].[EntityLogicIUD] ON ref.Entity;
UPDATE ref.Entity
SET isVersioning = 1
WHERE id = -1610612501;
ENABLE TRIGGER [ref].[EntityLogicIUD] ON ref.Entity;


DISABLE TRIGGER [ref].[EntityFieldIUD] ON ref.EntityField;
SET IDENTITY_INSERT ref.EntityField ON;

INSERT INTO ref.EntityField (id	,	NAME,	Caption,	[DESCRIPTION],	[IDENTITY],	idEntityFieldType,	idCalculatedFieldType,	SIZE,	[Precision],	Expression,	idEntityLink,	idOwnerField,	idForeignKeyType,	AllowNull,	isCaption,	isDescription,	DefaultValue,	idFieldDefaultValueType,	isSystem,	[READONLY],	isHidden,	RegExpValidator,	Tooltip)
VALUES (-1610610628,		'ValidityFrom',	'Дата начала действия',	NULL,	-1610612501,	24,	NULL,	NULL,	NULL,	NULL,	NULL,	NULL,	NULL,	1,	0,	0,	NULL,	NULL,	1,	0,	0,	NULL,	NULL),
(-1610610627,		'ValidityTo',	'Дата окончания действия',	NULL,	-1610612501,	24,	NULL,	NULL,	NULL,	NULL,	NULL,	NULL,	NULL,	1,	0,	0,	NULL,	NULL,	1,	0,	0,	NULL,	NULL),
(-1610610626,		'idRoot',	'Cсылка на корень',	NULL,	-1610612501,	7,	NULL,	NULL,	NULL,	NULL,	-1610612501,	NULL,	NULL,	1,	0,	0,	NULL,	NULL,	1,	0,	1,	NULL,	NULL),
(-1610610625,		'idActualItem',	'Актуальный элемент',	NULL,	-1610612501,	7,	4,	NULL,	NULL,	NULL,	-1610612501,	NULL,	NULL,	1,	0,	0,	NULL,	NULL,	1,	0,	1,	NULL,	NULL);

SET IDENTITY_INSERT ref.EntityField OFF;
ENABLE TRIGGER [ref].[EntityFieldIUD] ON ref.EntityField;
*/

DELETE FROM [ref].[EntityField] WHERE id=-1610610628;
DELETE FROM [ref].[EntityField] WHERE id=-1610610627;
DELETE FROM [ref].[EntityField] WHERE id=-1610610626;
DELETE FROM [ref].[EntityField] WHERE id=-1610610625;