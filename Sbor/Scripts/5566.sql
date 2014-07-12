Update [ref].[OrganizationRightExtension]
     Set SqlTemplate = 'SELECT sp.[id] as id , (SELECT [id] FROM [ref].[Entity] WHERE name=''StateProgram'') as [idEntity] FROM [doc].[StateProgram] sp
      WHERE sp.[id]= {0}.idMasterDoc
      UNION
      SELECT sp.[id] as id , (SELECT [id] FROM [ref].[Entity] WHERE name=''StateProgram'') as [idEntity] FROM [doc].[StateProgram] sp
      Inner Join  dbo.[GetDocumentVersionIds]({0}.id, (SELECT Top 1 [id] FROM [ref].[Entity] WHERE name=''{0}'')) cte  on cte.id = sp.idMasterDoc 
      UNION
      SELECT lgtp.[id] as id , (SELECT [id] FROM [ref].[Entity] WHERE name=''LongTermGoalProgram'') as [idEntity] FROM [doc].[LongTermGoalProgram] lgtp
      Inner Join  dbo.[GetDocumentVersionIds]({0}.id, (SELECT Top 1 [id] FROM [ref].[Entity] WHERE name=''{0}'')) cte  on cte.id = lgtp.idMasterStateProgram
      UNION
      SELECT aspb.[id] as id , (SELECT [id] FROM [ref].[Entity] WHERE name=''ActivityOfSBP'') as [idEntity] FROM [doc].[ActivityOfSBP] aspb
	  Inner Join  dbo.[GetDocumentVersionIds]({0}.id, (SELECT Top 1 [id] FROM [ref].[Entity] WHERE name=''{0}'')) cte  on cte.id = aspb.idMasterDoc
      UNION
      SELECT aspb.[id] as id , (SELECT [id] FROM [ref].[Entity] WHERE name=''ActivityOfSBP'') as [idEntity] FROM [doc].[ActivityOfSBP] aspb
         Inner Join [doc].[StateProgram] sp on aspb.idMasterDoc = sp.[id]
    	 Inner Join  dbo.[GetDocumentVersionIds]([{0}].id, (SELECT Top 1 [id] FROM [ref].[Entity] WHERE name=''{0}'')) cte  on cte.id = sp.idMasterDoc
      UNION
       SELECT lgtp.[id] as id , (SELECT [id] FROM [ref].[Entity] WHERE name=''LongTermGoalProgram'') as [idEntity] FROM [doc].[LongTermGoalProgram] lgtp
       WHERE  lgtp.idMasterStateProgram IN (
      SELECT lgtp.[id] as id  FROM [doc].[StateProgram] lgtp
      Inner Join  dbo.[GetDocumentVersionIds]([{0}].id, (SELECT Top 1 [id] FROM [ref].[Entity] WHERE name=''{0}'')) cte  on cte.id = lgtp.idMasterDoc
	  )'
Where id = -1744830434