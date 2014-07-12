
set identity_insert ref.Entityfield on;

select * into Entityfield_copy from ref.Entityfield where ID in ( -1610610625 , -1610610626, -1610610627, -1610610628)

delete ref.Entityfield where ID in ( -1610610625 , -1610610626, -1610610627, -1610610628)

INSERT INTO ref.Entityfield ([id],[Name],[Caption],[Description],[idEntity],[idEntityFieldType],[idCalculatedFieldType],[Size],[Precision],[Expression],[idEntityLink],[idOwnerField],[idForeignKeyType],[AllowNull],[isCaption],[isDescription],[DefaultValue],[idFieldDefaultValueType],[isSystem],[ReadOnly],[isHidden],[RegExpValidator],[Tooltip])
select EF.[id],EF.[Name],EF.[Caption],EF.[Description],EF.[idEntity],EF.[idEntityFieldType],EF.[idCalculatedFieldType],EF.[Size],EF.[Precision],EF.[Expression],EF.[idEntityLink],EF.[idOwnerField],EF.[idForeignKeyType],EF.[AllowNull],EF.[isCaption],EF.[isDescription],EF.[DefaultValue],EF.[idFieldDefaultValueType],EF.[isSystem],EF.[ReadOnly],EF.[isHidden],EF.[RegExpValidator],EF.[Tooltip]  from Entityfield_copy as EF 

drop table Entityfield_copy;

set identity_insert ref.Entityfield off;

set identity_insert tp.FormElements on;
INSERT INTO tp.FormElements (id,idOwner,idEntityField,idParent,[Order],Name) VALUES (-1610612624,-1610612700,-1610610628,NULL,50,'Дата начала действия')
INSERT INTO tp.FormElements (id,idOwner,idEntityField,idParent,[Order],Name) VALUES (-1610612622,-1610612701,-1610610628,-1610612633,0,'Дата начала действия')
INSERT INTO tp.FormElements (id,idOwner,idEntityField,idParent,[Order],Name) VALUES (-1610612623,-1610612700,-1610610627,NULL,60,'Дата окончания действия')
INSERT INTO tp.FormElements (id,idOwner,idEntityField,idParent,[Order],Name) VALUES (-1610612621,-1610612701,-1610610627,-1610612632,0,'Дата окончания действия')
set identity_insert tp.FormElements off;