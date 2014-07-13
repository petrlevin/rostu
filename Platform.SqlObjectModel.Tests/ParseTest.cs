using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class ParseTest
	{
		[Test]
        
		public void Test()
		{
            string str = @";SELECT [dbo].GetCaption(-1744830429,-1744830438) as [source];";

			var parser = new TSql100Parser(false);
			IList<ParseError> errors;
            IScriptFragment result = parser.Parse(new StringReader(@Statement), out errors);
			if (errors.Count > 0)
			{
				Console.WriteLine(@"Errors encountered: ""{0}""", errors[0].Message);
				return;
			}

			//приводим к ожидаемому типу, и смотрим что получилось
		    var batch = ((TSqlScript) result).Batches[0];
			TSqlStatement statement = batch.Statements[0];
		    var s  =statement.Render();

		    



		    //var t =(SubquerySpecification) ((SelectStatement)statement).QueryExpression;

		}

        private const string @Statement = @" SELECT [dbo].GetCaption(-1744830429,-1744830438) as [source];
WITH cte_source AS
(
SELECT id as [id ] ,-1543503791 as [idEntity] FROM doc.LongTermGoalProgram
UNION 
SELECT id as [id ] ,-1543503842 as [idEntity] FROM ref.Activity
)
SELECT CASE  WHEN [idEntity] in (-1543503791,-1543503798) THEN dbo.GetLastVersionId([original].[idEntity],[original].id) ELSE [original].id END
FROM cte_source as original

";


	    private const string @InsertTpStatement =
	        @"INSERT [tp].[RegisterActivity_Activity]  (id,idOwner,idIndicatorActivity_Volume,idActivity)
SELECT This.value('(id)[1]','INT'),@docId,This.value('(idIndicatorActivity_Volume)[1]','INT'),This.value('(idActivity)[1]','INT') FROM @stored.nodes('/root/tpActivity/item') AS T(This) 
WHERE This.value('(id)[1]','INT') NOT IN
(
SELECT id FROM [tp].[RegisterActivity_Activity] 
WHERE (idOwner = @docId)
)";


        private const string @DeleteStatement =
            @"DELETE FROM [tp].[RegisterActivity_Activity] 
WHERE @stored.exist('/root/tpActivity/item/id[text()  = sql:column("+"\"id\")]')=0 "+ @"
AND idOwner = @docId";


	    private const string @UpdateStatement =
	        @"UPDATE tp
SET tp.idIndicatorActivity_Volume = This.value('(idIndicatorActivity_Volume)[1]','INT') ,
tp.idContingent = This.value('(idContingent)[1]','INT') 
 FROM   [tp].[RegisterActivity_Activity] as tp INNER JOIN  @stored.nodes('/root/tpActivity/item') AS T(This)
ON [tp].[id] = [This].value('(id)[1]','INT')
--AND (This.value('(idIndicatorActivity_Volume)[1]','INT') !=  tp.idIndicatorActivity_Volume)

--AND (This.exist('(idIndicatorActivity_Volume)[(text()[1] cast as xs:int?) = -1543503849 ]') =0)
WHERE (This.exist('idIndicatorActivity_Volume[text()  = sql:column(" + "\"tp.idIndicatorActivity_Volume\") ]') =0) " +
	        @"
OR   ( ((tp.idContingent IS NULL) AND (This.exist('idContingent') =1)  ) OR ( (tp.idContingent IS NOT NULL) AND This.exist('idContingent[text()  = sql:column(" +
	        "\"tp.idContingent\") ]') =0))" + @"
AND tp.idOwner = @docId ";


        private const string @InsertStatement = @"
            DECLARE @input XML
            SELECT @input =  Data FROM [reg].[SerializedEntityItem] 
            WHERE [idTool] =-1744830438

            INSERT INTO [platform3].[reg].[SerializedEntityItem]
           ([Data]
           ,[idTool]
           ,[idToolEntity])
     VALUES
           



 ((SELECT id as 'id',
        IdDocStatus as 'IdDocStatus',
        
        (
			SELECT 
				id as id,
				idRegistryKeyActivity

				FROM [tp].[RegisterActivity_Activity] as tp
				WHERE tp.idOwner = doc.id
				FOR XML PATH('item') ,
				TYPE
				
			
        ) as 'Activity', 
        (
			SELECT 
				id,
				idRegistryKeyActivity

				FROM [tp].[RegisterActivity_Activity] as tp
				WHERE idOwner = doc.id
				FOR XML PATH('item') ,
				TYPE
				
			
        ) as 'OtherTablePart',
        (
             SELECT 
               idIndicatorActivity
               FROM [ml].[RegisterActivity_IndicatorActivity] as ml
               WHERE idRegisterActivity = doc.Id
				FOR XML PATH('') ,
				TYPE
               
        )as 'mlRegisterActivity_IndicatorActivity'
        
                
        FROM [doc].[RegisterActivity]  as doc
        WHERE [doc].[id]=@param
        FOR XML PATH('root') , TYPE
        
        
)
           ,-1744830438
           ,-1543503828)
";

	}
}
