update d set 
	d.idDocAUBUPeriodType_OFG  = ISNULL(t.idDocAUBUPeriodType_OFG  , 1), 
	d.idDocAUBUPeriodType_PFG1 = ISNULL(t.idDocAUBUPeriodType_PFG1 , 1), 
	d.idDocAUBUPeriodType_PFG2 = ISNULL(t.idDocAUBUPeriodType_PFG2 , 1)
from 
	doc.PlanActivity as d
	left join tp.SBP_PlanningPeriodsInDocumentsAUBU as t on 
		t.idOwner = d.idSBP and t.idBudget = d.idBudget
where
	d.idDocAUBUPeriodType_OFG is null 
	and d.idDocAUBUPeriodType_PFG1 is null
	and d.idDocAUBUPeriodType_PFG2 is null