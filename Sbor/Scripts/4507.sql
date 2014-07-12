delete 
from tp.PublicInstitutionEstimate_Expense
where not exists ( select null from tp.PublicInstitutionEstimate_Activity A where A.id = idMaster )

delete 
from tp.PublicInstitutionEstimate_Activity
where not exists ( select null from doc.PublicInstitutionEstimate P where P.id = idOwner )

delete 
from tp.PublicInstitutionEstimate_ActivityAUBU
where not exists ( select null from doc.PublicInstitutionEstimate P where P.id = idOwner )

delete 
from tp.PublicInstitutionEstimate_DistributionActivity
where not exists ( select null from doc.PublicInstitutionEstimate P where P.id = idOwner )

delete 
from tp.PublicInstitutionEstimate_IndirectExpenses
where not exists ( select null from doc.PublicInstitutionEstimate P where P.id = idOwner )

delete 
from tp.PublicInstitutionEstimate_DistributionMethod
where not exists ( select null from doc.PublicInstitutionEstimate P where P.id = idOwner )
;

declare @idEntityField int;

set @idEntityField = (select top 1 EF.id
			from ref.EntityField EF
				inner join ref.Entity E on E.id = EF.[idEntity]
			where E.Name = 'PublicInstitutionEstimate_Activity'
				and EF.Name = 'idOwner')

declare @idFK int;

set @idFK = (select top 1 EF.idForeignKeyType 
			from ref.EntityField EF
			where EF.id = @idEntityField)

select @idFK

if (@idFK is null or @idFK <> 3)
	update ref.EntityField
		set idForeignKeyType = 3
		where id = @idEntityField
