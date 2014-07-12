
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[tp].[FK_idEntityField_FormElements_EntityField]') AND parent_object_id = OBJECT_ID(N'[tp].[FormElements]'))
ALTER TABLE [tp].[FormElements] DROP CONSTRAINT [FK_idEntityField_FormElements_EntityField]

ALTER TABLE [tp].[FormElements]  WITH CHECK ADD  CONSTRAINT [FK_idEntityField_FormElements_EntityField] FOREIGN KEY([idEntityField])
REFERENCES [ref].[EntityField] ([id])
ON DELETE CASCADE

ALTER TABLE [tp].[FormElements] CHECK CONSTRAINT [FK_idEntityField_FormElements_EntityField]


