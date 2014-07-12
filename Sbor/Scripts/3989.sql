IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[doc].[FK_idParent_PlanActivity_LimitBudgetAllocations]') AND parent_object_id = OBJECT_ID(N'[doc].[PlanActivity]'))
ALTER TABLE [doc].[PlanActivity] DROP CONSTRAINT [FK_idParent_PlanActivity_LimitBudgetAllocations]
;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[doc].[FK_idParent_PlanActivity_PlanActivity]') AND parent_object_id = OBJECT_ID(N'[doc].[PlanActivity]'))
BEGIN
	ALTER TABLE [doc].[PlanActivity]  WITH CHECK ADD  CONSTRAINT [FK_idParent_PlanActivity_PlanActivity] FOREIGN KEY([idParent])
	REFERENCES [doc].[PlanActivity] ([id])
	;

	ALTER TABLE [doc].[PlanActivity] CHECK CONSTRAINT [FK_idParent_PlanActivity_PlanActivity]
	;
END
;
