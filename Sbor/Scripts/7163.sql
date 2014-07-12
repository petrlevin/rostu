update d
set d.idSBP_BlankActual = sbh.id
from doc.PlanActivity as d
join ref.SBP as sbp on sbp.id=d.IdSBP
join tp.SBP_BlankHistory as sbh on sbh.idOwner = sbp.idParent and sbh.idBudget=d.idBudget
join 
(select  idBudget, idOwner, idBlankType, max(DateCreate) as d
from tp.SBP_BlankHistory as sbh group by id , idBudget, idOwner, idBlankType) as maxsbh
on maxsbh.d=sbh.DateCreate and maxsbh.idBudget=sbh.idBudget and maxsbh.idBlankType=sbh.idBlankType and maxsbh.idOwner=sbh.idOwner
where d.idSBP_BlankActual is null and (sbp.idSBPType in (4,5) and sbh.idBlankType=3)
