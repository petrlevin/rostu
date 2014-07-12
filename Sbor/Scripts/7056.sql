UPDATE ref.Programmability
SET
	CreateCommand = 'CREATE FUNCTION [sbor].[VolumeExpensesImplementation] (@idSystemGoalElement int, @YearBudget int, @idVersion int, @PPO int, @IdBudget int, @idValueType int, @idValueType1 int)
RETURNS numeric(20,2)
AS

BEGIN 
declare @result numeric(20,2);

WITH Q (idSystemGoalElement,idSystemGoalElement_Parent) AS 
		(
		SELECT idSystemGoalElement,idSystemGoalElement_Parent
		FROM reg.AttributeOfSystemGoalElement as AOSGE
		WHERE AOSGE.idSystemGoalElement = @idSystemGoalElement 
		AND AOSGE.idTerminator IS NULL
		UNION ALL 
		SELECT AOSGE.idSystemGoalElement,AOSGE.idSystemGoalElement_Parent
		FROM reg.AttributeOfSystemGoalElement as AOSGE
		INNER JOIN Q 
		ON AOSGE.idSystemGoalElement_Parent = Q.idSystemGoalElement 
		AND AOSGE.idTerminator IS NULL
		)
		
		SELECT	 @result = SUM(LVA.Value)
		FROM reg.TaskCollection AS TC
			INNER JOIN (
						SELECT DISTINCT	 TV.idSystemGoalElement
										,TV.idTaskCollection 
										
							FROM Q 
						INNER JOIN reg.TaskVolume AS TV --объем задач
							ON Q.idSystemGoalElement = TV.idSystemGoalElement
							AND TV.idValueType = 1
							AND TV.idTerminator IS NULL
							AND TV.idVersion = @idVersion
							AND TV.idPublicLegalFormation = @PPO
						) AS TAB
				ON TC.id = TAB.idTaskCollection
			INNER JOIN reg.LimitVolumeAppropriations AS LVA --Объемы финансовых средств
				ON LVA.idTaskCollection = TC.id 
				AND LVA.idValueType IN (@idValueType,@idValueType1)
				AND LVA.idVersion = @idVersion
				AND LVA.idPublicLegalFormation = @PPO
				AND LVA.idBudget = @IdBudget
			INNER JOIN ref.HierarchyPeriod AS HP 
				ON HP.id = LVA.idHierarchyPeriod
				AND (YEAR(HP.DateEnd) = @YearBudget )
			GROUP BY HP.DateEnd
RETURN @result
END'
WHERE id = -1275068363
