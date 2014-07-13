using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Procedures
{
	/// <summary>
	/// класс описывающий реализацию процедуры создания триггера, проверяющего налицие циклов в иерархии
	/// </summary>
	public class CreateOrAlterCheckCycleHierarchyTrigger
	{
		#region Private Property
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		private const string _checkEqual =
			"{3} EXISTS(SELECT 1 FROM INSERTED a INNER JOIN [{0}].[{1}] b on [b].[id]=[a].[id] WHERE [b].[id]=[b].[{2}])";

		private const string _checkCycle = @"WITH cte AS(
SELECT [a].[{2}], [a].[id], '_'+CAST([a].[id] AS VARCHAR(max))+'_' as cycle FROM [{0}].[{1}] a INNER JOIN INSERTED b ON [b].[id]=[a].[id]
UNION ALL
SELECT [a].[{2}], CASE WHEN [b].[cycle] LIKE '%_'+CAST([a].[id] AS VARCHAR(30))+'_%' THEN -1 ELSE [a].[id] END AS child, CAST([b].[cycle] AS varchar(max)) + cast([a].[id] as VARCHAR(max)) + '_' AS cycle
FROM [{0}].[{1}] a 
	INNER JOIN cte b on [a].[{2}]=[b].[id]
	INNER JOIN [{0}].[{1}] c on [c].[id]=[b].[{2}]
)
INSERT INTO @tmp SELECT [id] FROM cte WHERE [id]=-1;

IF EXISTS(SELECT 1 FROM @tmp)
	RAISERROR(50005, 16, 1);

 DELETE FROM @tmp;
";
		private const string _textTrigger = @"CREATE TRIGGER [{0}].[{1}_CheckCycleHierarchy]
   ON  [{0}].[{1}]
   AFTER INSERT, UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

    IF {2}
    BEGIN
		RAISERROR(50005, 16, 1);
	END

	DECLARE @tmp table (id int);
	
	{3}
END";
		#endregion

		#region private Methods
		/// <summary>
		/// Удаление триггера
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static void _dropTrigger(Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(
				string.Format(
					"IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_AllowGenericLink_Delete]')) DROP TRIGGER [{0}].[{1}_AllowGenericLink_Delete]",
					entity.Schema, entity.Name));
		}

		/// <summary>
		/// Создание триггера
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static void _createTrigger(Entity entity)
		{
			StringBuilder checkEqualValue = new StringBuilder();
			StringBuilder checkCycle = new StringBuilder();
			bool isFirst = true;
			foreach (IEntityField entityField in entity.RealFields.Where(a => !a.IdCalculatedFieldType.HasValue 
                                                                                && a.EntityFieldType == EntityFieldType.Link
                                                                                && a.IdEntityLink.HasValue
                                                                                && a.IdEntityLink == a.IdEntity))
			{
				checkCycle.AppendLine(string.Format(_checkCycle, entity.Schema, entity.Name, entityField.Name));
				if (isFirst)
				{
					checkEqualValue.AppendLine(
						string.Format(_checkEqual, entity.Schema, entity.Name, entityField.Name, ""));
					isFirst = false;
				} else
				{
					checkEqualValue.AppendLine(string.Format(_checkEqual, entity.Schema, entity.Name, entityField.Name, "OR"));
				}
			}
			if (isFirst)
				return;
			_sqlCmd.ExecuteNonQuery(string.Format(_textTrigger, entity.Schema, entity.Name, checkEqualValue.ToString(),
			                                      checkCycle.ToString()));
		}
		#endregion
		/// <summary>
		/// Метод создания триггера
		/// </summary>
		/// <param name="idEntity">Идентификатор сущности</param>
		[SqlProcedure]
		public static void Exec(int idEntity)
		{
			try
			{
				Entity entity = Objects.ById<Entity>(idEntity);
				_dropTrigger(entity);

				if (!entity.RealFields.Any(a => !a.IdCalculatedFieldType.HasValue && a.EntityFieldType == EntityFieldType.Link && a.IdEntityLink.HasValue && a.IdEntityLink == a.IdEntity))
					return;
				_createTrigger(entity);
			}
			catch
			{
				return;
			}
			
		}
	}
}
