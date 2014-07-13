using System;
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
	public class GetDescription
	{
	    /// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Список типов поля - общих ссылок
		/// </summary>
		private static readonly EntityFieldType[] GenericLink = new[]
				{
					EntityFieldType.ReferenceEntity,
					EntityFieldType.ToolEntity,
					EntityFieldType.TablepartEntity,
					EntityFieldType.DocumentEntity
				};


		/// <summary>
		/// Получение наименования для общей ссылки
		/// </summary>
		/// <param name="idEntity">Идентифкатор сущности</param>
		/// <param name="idItem">Идентификатор элемента</param>
		[SqlFunction(DataAccess = DataAccessKind.Read)]
		public static String Get(Int32 idEntity, Int32 idItem)
		{
			IEntity entity = Objects.ById<Entity>(idEntity);
			IEntityField descriptionField = entity.DescriptionField;

		    if (descriptionField == null)
		        return String.Empty;

			StringBuilder textCommand = new StringBuilder("SELECT [{0}].[{1}] FROM ");
			string result = "";
			if (descriptionField.EntityFieldType == EntityFieldType.String || descriptionField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand = new StringBuilder();
				textCommand.AppendFormat("SELECT {0} FROM [{1}].[{2}] WHERE [id]={3}", descriptionField.Name, entity.Schema, entity.Name, idItem);
			}
			else if (descriptionField.EntityFieldType == EntityFieldType.Int || descriptionField.EntityFieldType == EntityFieldType.TinyInt || descriptionField.EntityFieldType == EntityFieldType.SmallInt || descriptionField.EntityFieldType == EntityFieldType.BigInt)
			{
				textCommand = new StringBuilder();
				textCommand.AppendFormat("SELECT CAST({0} AS NVARCHAR) FROM [{1}].[{2}] WHERE [id]={3}", descriptionField.Name, entity.Schema, entity.Name, idItem);
			}
            else if (descriptionField.EntityFieldType == EntityFieldType.File)
            {
                //Вообще конечно довольно странный выбор для поля-описания...
                textCommand = new StringBuilder();
                textCommand.AppendFormat("SELECT 'Файл'");

            }
            else if (descriptionField.IsFieldWithForeignKey())
            {
                const char alias = 'a';
                textCommand.AppendFormat("[{0}].[{1}] [{2}] ", entity.Schema, entity.Name, alias);
                string joinType = descriptionField.AllowNull ? Helper.LeftOuterJoin : Helper.InnerJoin;
                _reqursiveDescription(textCommand, descriptionField, joinType, alias);
                textCommand.AppendFormat(" WHERE [a].[id]={0}", idItem);
            }
            else if (descriptionField.EntityFieldType == EntityFieldType.ReferenceEntity ||
                     descriptionField.EntityFieldType == EntityFieldType.DocumentEntity ||
                     descriptionField.EntityFieldType == EntityFieldType.TablepartEntity ||
                     descriptionField.EntityFieldType == EntityFieldType.ToolEntity)
            {
                textCommand = new StringBuilder();
                textCommand.AppendFormat(
                    "SELECT [{0}].[{1}] as Id, [{0}].[{1}Entity] as [IdEntity] FROM [{2}].[{3}] [a] WHERE [a].[id]={4}",
                    "a",
                    descriptionField.Name, entity.Schema, entity.Name, idItem);
                MyResult myResult = _sqlCmd.SelectFirst<MyResult>(textCommand.ToString());
                if (myResult != null)
                {
                    result = Get(myResult.IdEntity, myResult.Id);
                }
                textCommand = new StringBuilder("");
            }
            else
            {
                throw new Exception("Не реализовано для " + descriptionField.EntityFieldType);
            }
		    if (!string.IsNullOrEmpty(textCommand.ToString()))
			{
				using (SqlConnection connection = SqlCmd.ContextConnection)
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(textCommand.ToString(), connection))
					{
						object resObject = command.ExecuteScalar();
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

		/// <summary>
		/// 
		/// </summary>
		public class MyResult
		{
			/// <summary>
			/// Идентификатор элемента
			/// </summary>
			public int Id { get; set; }
			
            /// <summary>
			/// Идентификатор сущности
			/// </summary>
			public int IdEntity { get; set; }
		}

	    /// <summary>
	    /// Рекурсивня функция для построения запроса получения Description
	    /// </summary>
	    /// <param name="textCommand">Изменяемы текст команды</param>
        /// <param name="descriptionField">Description поле</param>
	    /// <param name="joinType">Тип соединения</param>
	    /// <param name="alias">Алиас таблицы</param>
	    private static void _reqursiveDescription(StringBuilder textCommand, IEntityField descriptionField, string joinType, char alias)
		{
			char nextAlias = alias;
			nextAlias++;
			IEntity nextEntity = descriptionField.EntityLink;
			IEntityField nextDescriptionField = nextEntity.DescriptionField;
			textCommand.AppendFormat("{0} [{1}].[{2}] [{3}] ON [{4}].[{5}]=[{3}].[id] ", joinType, nextEntity.Schema,
									 nextEntity.Name, nextAlias, alias, descriptionField.Name);
			if (nextDescriptionField.EntityFieldType == EntityFieldType.String || nextDescriptionField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand.Replace("{0}", nextAlias.ToString());
				textCommand.Replace("{1}", nextDescriptionField.Name);
			}
            else if ( nextDescriptionField.IsFieldWithForeignKey() )
			{
				string nextJoinType = joinType == Helper.LeftOuterJoin
										  ? Helper.LeftOuterJoin
										  : (nextDescriptionField.AllowNull ? Helper.LeftOuterJoin : Helper.InnerJoin);
				_reqursiveDescription(textCommand, nextDescriptionField, nextJoinType, nextAlias);
			}
			else
			{
				throw new Exception("Не реализовано для " + nextDescriptionField.EntityFieldType);
			}
		}
	}
}
