update tv
set tv.idSystemGoalElement = sge.id
from reg.TaskVolume as tv
inner join reg.TaskCollection as tc on tc.id=tv.idTaskCollection
inner join tp.ActivityOfSBP_Activity as atp on 
				atp.idOwner = tv.idRegistrator and 
				tc.idActivity=atp.idActivity and
				tv.idSBP = atp.idSBP and 
				(tc.idContingent is null and atp.idContingent is null or tc.idContingent = atp.idContingent)
inner join tp.ActivityOfSBP_SystemGoalElement as tpsge on tpsge.id=atp.idMaster
inner join reg.SystemGoalElement as sge on sge.idSystemGoal = tpsge.idSystemGoal
where idSystemGoalElement not in (select id from reg.SystemGoalElement)

update tv
set tv.idSystemGoalElement = null
from reg.TaskVolume as tv
where idSystemGoalElement not in (select id from reg.SystemGoalElement)
