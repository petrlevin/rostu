-- это копия скрипта 5790.sql (по техническим причинам он не выполнился на большинстве баз в необходимое время)

update d
set d.idSBP_BlankActual = sbh.id
from doc.PlanActivity as d
join ref.SBP as sbp on sbp.id=d.IdSBP
join tp.SBP_BlankHistory as sbh on sbh.idOwner = sbp.idParent and sbh.idBudget=d.idBudget
and ((sbp.idSBPType = 3 and sbh.idBlankType=4) or (sbp.idSBPType = 5 and sbh.idBlankType=3))

update d
set d.idSBP_BlankActual = sbh.id
from doc.FinancialAndBusinessActivities as d
join ref.SBP as sbp on sbp.id=d.IdSBP
join tp.SBP_BlankHistory as sbh on sbh.idOwner = sbp.idParent and sbh.idBudget=d.idBudget
and sbh.idBlankType=5

update d
set d.idSBP_BlankActual = sbh.id
from doc.LimitBudgetAllocations as d
join ref.SBP as sbp on sbp.id=d.IdSBP
join tp.SBP_BlankHistory as sbh on sbh.idBudget=d.idBudget
and ((sbp.idSBPType = 1 and sbh.idBlankType=1 and sbh.idOwner = sbp.id) 
  or (sbp.idSBPType = 2 and sbh.idBlankType=2 and sbh.idOwner = sbp.idParent))

update d
set d.idSBP_BlankActual = sbh.id
from doc.PublicInstitutionEstimate as d
join ref.SBP as sbp on sbp.id=d.IdSBP
join tp.SBP_BlankHistory as sbh on sbh.idOwner = sbp.idParent and sbh.idBudget=d.idBudget
and sbh.idBlankType=6

update d
set d.idSBP_BlankActualAuBu = sbh.id
from doc.PublicInstitutionEstimate as d
join ref.SBP as sbp on sbp.id=d.IdSBP and sbp.isFounder=1
join tp.SBP_BlankHistory as sbh on sbh.idOwner = sbp.id and sbh.idBudget=d.idBudget
and sbh.idBlankType=5

;
if exists ( select * from sys.all_columns
where object_id = OBJECT_ID('tp.PublicInstitutionEstimate_Activity')
and name = 'idActivityType' )
	ALTER TABLE [tp].[PublicInstitutionEstimate_Activity] DROP COLUMN [idActivityType]
;