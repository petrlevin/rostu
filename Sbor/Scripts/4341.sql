
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[tp].[FK_idOwner_PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense_PublicInstitutionEstimate]') AND parent_object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense]'))
ALTER TABLE [tp].[PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense] DROP CONSTRAINT [FK_idOwner_PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense_PublicInstitutionEstimate]
;

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[tp].[FK_idOwner_PublicInstitutionEstimate_DistributionActivity_PublicInstitutionEstimate]') AND parent_object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_DistributionActivity]'))
ALTER TABLE [tp].[PublicInstitutionEstimate_DistributionActivity] DROP CONSTRAINT [FK_idOwner_PublicInstitutionEstimate_DistributionActivity_PublicInstitutionEstimate]
;

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[tp].[FK_idOwner_PublicInstitutionEstimate_DistributionAdditionalParam_PublicInstitutionEstimate]') AND parent_object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_DistributionAdditionalParam]'))
ALTER TABLE [tp].[PublicInstitutionEstimate_DistributionAdditionalParam] DROP CONSTRAINT [FK_idOwner_PublicInstitutionEstimate_DistributionAdditionalParam_PublicInstitutionEstimate]
;

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[tp].[FK_idOwner_PublicInstitutionEstimate_Expense_PublicInstitutionEstimate]') AND parent_object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_Expense]'))
ALTER TABLE [tp].[PublicInstitutionEstimate_Expense] DROP CONSTRAINT [FK_idOwner_PublicInstitutionEstimate_Expense_PublicInstitutionEstimate]
;


IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[tp].[FK_idOwner_PublicInstitutionEstimate_FounderAUBUExpense_PublicInstitutionEstimate]') AND parent_object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_FounderAUBUExpense]'))
ALTER TABLE [tp].[PublicInstitutionEstimate_FounderAUBUExpense] DROP CONSTRAINT [FK_idOwner_PublicInstitutionEstimate_FounderAUBUExpense_PublicInstitutionEstimate]
;

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[tp].[FK_idOwner_PublicInstitutionEstimate_Activity_PublicInstitutionEstimate]') AND parent_object_id = OBJECT_ID(N'[tp].[PublicInstitutionEstimate_Activity]'))
ALTER TABLE [tp].[PublicInstitutionEstimate_Activity] DROP CONSTRAINT [FK_idOwner_PublicInstitutionEstimate_Activity_PublicInstitutionEstimate]
;

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