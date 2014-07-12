IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE name = 'DEFAULT_StateProgramGoalIndicatorValue_byApproved' AND type = 'D')
BEGIN
ALTER TABLE [rep].[StateProgramGoalIndicatorValue] DROP CONSTRAINT [DEFAULT_StateProgramGoalIndicatorValue_byApproved]
END;

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE name = 'DEFAULT_StateProgram_TargetsCopositionAndVaues_repeatTableHeader' AND type = 'D')
BEGIN
ALTER TABLE [rep].[StateProgramGoalIndicatorValue] DROP CONSTRAINT [DEFAULT_StateProgram_TargetsCopositionAndVaues_repeatTableHeader]
END;

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE name = 'DEFAULT_StateProgram_TargetsCopositionAndVaues_DateReport' AND type = 'D')
BEGIN
ALTER TABLE [rep].[StateProgramGoalIndicatorValue] DROP CONSTRAINT [DEFAULT_StateProgram_TargetsCopositionAndVaues_DateReport]
END;

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[rep].[FK_idVersion_StateProgram_TargetsCopositionAndVaues_Version]') AND parent_object_id = OBJECT_ID(N'[rep].[StateProgramGoalIndicatorValue]'))
ALTER TABLE [rep].[StateProgramGoalIndicatorValue] DROP CONSTRAINT [FK_idVersion_StateProgram_TargetsCopositionAndVaues_Version];

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[rep].[FK_idProgram_StateProgram_TargetsCopositionAndVaues_Program]') AND parent_object_id = OBJECT_ID(N'[rep].[StateProgramGoalIndicatorValue]'))
ALTER TABLE [rep].[StateProgramGoalIndicatorValue] DROP CONSTRAINT [FK_idProgram_StateProgram_TargetsCopositionAndVaues_Program];

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[rep].[FK_idPublicLegalFormation_StateProgram_TargetsCopositionAndVaues_PublicLegalFormation]') AND parent_object_id = OBJECT_ID(N'[rep].[StateProgramGoalIndicatorValue]'))
ALTER TABLE [rep].[StateProgramGoalIndicatorValue] DROP CONSTRAINT [FK_idPublicLegalFormation_StateProgram_TargetsCopositionAndVaues_PublicLegalFormation];