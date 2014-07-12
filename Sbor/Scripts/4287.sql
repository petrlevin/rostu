UPDATE [reg].[AttributeOfProgram]
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL


UPDATE reg.AttributeOfSystemGoalElement
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL


UPDATE reg.GoalTarget
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL


UPDATE reg.Program
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL

UPDATE reg.Program_ResourceMaintenance
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL

UPDATE reg.SystemGoalElement
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL

UPDATE reg.TaskIndicatorQuality
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL

UPDATE reg.TaskVolume
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL

UPDATE reg.ValuesGoalTarget
   SET [idTerminateOperation] = null
 WHERE idTerminator IS NULL



