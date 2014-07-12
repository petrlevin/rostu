update ref.EntityField 
set 
	idCalculatedFieldType=3,
	Expression = 'doc.StateProgram_CaptionDocumentRegistratorSubProgram(idOwner, idSBP, idDocType, idSystemGoal)'
where 
	Name = 'Document' and id in (-2013260428,-2013260427)

update ref.EntityField 
set 
	idCalculatedFieldType=3,
	Expression = 'doc.LongTermGoalProgram_CaptionDocumentRegistratorSubProgram(idOwner, idSBP, idDocType, idSystemGoal)'
where 
	Name = 'Document' and id = -2013260426