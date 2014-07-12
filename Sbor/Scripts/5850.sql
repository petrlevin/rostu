;
update tq
set tq.idProgram = P.id
from reg.TaskIndicatorQuality tq
inner join reg.Program P on ( P.idRegistratorEntity =  -1543503797 
								and P.idTerminator is null 
								and P.idRegistrator in ( select id from dbo.GetParents(tq.idRegistrator, -1543503471, 'ActivityOfSBP', 'idParent', 1)) )   
where tq.idProgram is null
	  and tq.idRegistratorEntity = -1543503797
;