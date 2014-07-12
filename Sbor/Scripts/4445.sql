declare @docEntityId int

set @docEntityId = (select id from ref.Entity E
						where E.Name = 'LimitBudgetAllocations')

update reg.LimitVolumeAppropriations
set DateCommit = 
(select top 1 L.[Date] from doc.LimitBudgetAllocations L
	where L.id = idRegistrator)
where idRegistratorEntity = @docEntityId and 
	  DateCommit is not null
