UPDATE doc.LimitBudgetAllocations 
SET    [idSBPType] = (SELECT SBPT.id
						FROM ref.SBP AS SBP
						INNER JOIN enm.SBPType AS SBPT
							ON SBP.idSBPType = SBPT.id
                        WHERE  SBP.id = doc.LimitBudgetAllocations.idSBP);

delete from enm.AggregateFunction where id>100