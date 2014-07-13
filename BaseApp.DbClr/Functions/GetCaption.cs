using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Functions
{
	/// <summary>
	/// Получения наименования для общей ссылки
	/// </summary>
	public class GetCaption
	{
	    /// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd SqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

	    /// <summary>
		/// Получение наименования для общей ссылки
		/// </summary>
		/// <param name="idEntity">Идентифкатор сущности</param>
		/// <param name="idItem">Идентификатор элемента</param>
		[SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
		public static String Get(Int32 idEntity, Int32 idItem)
		{
			IEntity entity = Objects.ById<Entity>(idEntity);
			IEntityField captionField = entity.CaptionField;
			StringBuilder textCommand = new StringBuilder("SELECT [{0}].[{1}] FROM ");
			string result = "";
			if (captionField.EntityFieldType == EntityFieldType.String || captionField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand = new StringBuilder();
				textCommand.AppendFormat("SELECT {0} FROM [{1}].[{2}] WHERE [id]={3}", captionField.Name, entity.Schema, entity.Name, idItem);
			}
			else if (captionField.EntityFieldType == EntityFieldType.Int || captionField.EntityFieldType == EntityFieldType.TinyInt || captionField.EntityFieldType == EntityFieldType.SmallInt || captionField.EntityFieldType == EntityFieldType.BigInt)
			{
				textCommand = new StringBuilder();
				textCommand.AppendFormat("SELECT CAST({0} AS NVARCHAR) FROM [{1}].[{2}] WHERE [id]={3}", captionField.Name, entity.Schema, entity.Name, idItem);
			}
            else if (captionField.EntityFieldType == EntityFieldType.File)
            {
                // Пока пусть будет так
                textCommand = new StringBuilder();
                textCommand.AppendFormat("SELECT 'Файл'");
            }
            else if (captionField.IsFieldWithForeignKey())
            {
                const char alias = 'a';
                textCommand.AppendFormat("[{0}].[{1}] [{2}] ", entity.Schema, entity.Name, alias);
                string joinType = captionField.AllowNull ? Helper.LeftOuterJoin : Helper.InnerJoin;
                _reqursiveCaption(textCommand, entity, captionField, joinType, alias);
                textCommand.AppendFormat(" WHERE [{1}].[id]={0}", idItem, alias);
            }
            else if (captionField.EntityFieldType == EntityFieldType.ReferenceEntity ||
                     captionField.EntityFieldType == EntityFieldType.DocumentEntity ||
                     captionField.EntityFieldType == EntityFieldType.TablepartEntity ||
                     captionField.EntityFieldType == EntityFieldType.ToolEntity)
            {
                textCommand = new StringBuilder();
                textCommand.AppendFormat(
                    "SELECT [{0}].[{1}] as Id, [{0}].[{1}Entity] as [IdEntity] FROM [{2}].[{3}] [a] WHERE [a].[id]={4}",
                    "a",
                    captionField.Name, entity.Schema, entity.Name, idItem);
                MyResult myResult = SqlCmd.SelectFirst<MyResult>(textCommand.ToString());
                if (myResult != null)
                {
                    result = Get(myResult.IdEntity, myResult.Id);
                }
                textCommand = new StringBuilder("");
            }
            else
            {
                throw new Exception("Не реализовано для " + captionField.EntityFieldType);
            }

	        if (!string.IsNullOrEmpty(textCommand.ToString()))
			{
				using (SqlConnection connection = SqlCmd.ContextConnection)
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(textCommand.ToString(), connection))
					{
					    object resObject;
					    try
					    {
                            resObject = command.ExecuteScalar();
					    }
					    catch (Exception e)
					    {
                            throw new Exception("Ошбика выполнения команды: " + textCommand + "; "+ e);
					    }
						
						if (resObject is DBNull || resObject == null)
							result = "";
						else
						{
							result = (string) resObject;
						}
					}
					connection.Close();
				}
			}
			return result;
		}

		public class MyResult
		{
			public int Id { get; set; }
			public int IdEntity { get; set; }
		}

		/// <summary>
		/// Рекурсивня функция для построения запроса получения Caption
		/// </summary>
		/// <param name="textCommand">Изменяемы текст команды</param>
		/// <param name="entity">Сущность</param>
		/// <param name="captionField">Caption поле</param>
		/// <param name="joinType">Тип соединения</param>
		/// <param name="alias">Алиас таблицы</param>
		private static void _reqursiveCaption(StringBuilder textCommand, IEntity entity, IEntityField captionField, string joinType, char alias)
		{
			char nextAlias = alias;
			nextAlias++;
			IEntity nextEntity = captionField.EntityLink;
			IEntityField nextCaptionField = nextEntity.CaptionField;
			textCommand.AppendFormat("{0} [{1}].[{2}] [{3}] ON [{4}].[{5}]=[{3}].[id] ", joinType, nextEntity.Schema,
									 nextEntity.Name, nextAlias, alias, captionField.Name);
			if (nextCaptionField.EntityFieldType == EntityFieldType.String || nextCaptionField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand.Replace("{0}", nextAlias.ToString() );
				textCommand.Replace("{1}", nextCaptionField.Name);
			}
			else if ( nextCaptionField.IsFieldWithForeignKey() )
			{
				string nextJoinType = joinType == Helper.LeftOuterJoin
										  ? Helper.LeftOuterJoin
										  : (nextCaptionField.AllowNull ? Helper.LeftOuterJoin : Helper.InnerJoin);
				_reqursiveCaption(textCommand, nextEntity, nextCaptionField, nextJoinType, nextAlias);
			}
			else
			{
				throw new Exception("Не реализовано для " + nextCaptionField.EntityFieldType);
			}
		}

		
	}
}
