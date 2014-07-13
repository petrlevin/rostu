using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Common;
using Platform.Dal.Exceptions;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Обработка SqlException
    /// </summary>
    public class SqlExceptionHandler
    {
        #region Private Fields
        private readonly SqlConnection _connection = IoC.Resolve<SqlConnection>("DbConnection");

        private readonly Exception _exception;
        #endregion

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; private set; }
        private SqlExceptionHandler()
        {
        }

        /// <summary>
        /// Обработка ошибок для мультилинка
        /// </summary>
        /// <param name="exception">Ошибка</param>
        /// <param name="rightField">Правое поле мультилинка</param>
        /// <param name="addValue">Идентификатор добавляемого элемента</param>
        public SqlExceptionHandler(SqlException exception, IEntityField rightField, int addValue)
        {
            switch (exception.Number)
            {


                case 2627:
                    {
                        string textCommand = string.Format("SELECT {0} FROM [{1}].[{2}] WHERE [id]={3}",
                                                           rightField.EntityLink.CaptionField.Name, rightField.EntityLink.Schema,
                                                           rightField.EntityLink.Name, addValue);
                        SqlCmd sqlCmd = new SqlCmd(_connection);
                        Message = "Следующие элементы уже добавлены:<br> '" + sqlCmd.ExecuteScalar<string>(textCommand) + "'";
                        break;
                    }
                case 515:
                    {
                        Match match = Regex.Matches(exception.Message, "\'[^\']+\'")[0];
                        string fieldName =
                            ((EntityField)rightField).Entity.Fields.Single(
                                a => a.Name.Equals(match.Value, StringComparison.OrdinalIgnoreCase)).Caption;
                        Message = string.Format("Поле '{0}' не может быть пустым", fieldName);
                        break;
                    }
                case 547:
                    {
                        Match match = Regex.Matches(exception.Message, "\"[^\"]+\"")[2];
                        SqlCmd sqlCmd = new SqlCmd(_connection);
                        string name = sqlCmd.ExecuteScalar<string>("SELECT Caption FROM [ref].[Entity] WHERE Name='" +
                                                                   match.Value.Split(new char[] { '.' })[1].Replace("\"", "") + "'");
                        Message = string.Format("Указана ссылка на несуществующий элемент в таблице '{0}'", name);
                        break;
                    }
                default:
                    throw exception;
            }
        }

        /// <summary>
        /// Оюбработка исключения
        /// </summary>
        /// <returns></returns>
        public Exception Process()
        {
            if (_exception is DbUpdateException)
            {
                DbUpdateException handleException = (_exception as DbUpdateException);
                SqlException innerException = _getInnerSqlException(handleException);
                if (innerException == null)
                    return handleException;
				SqlCmd sqlCmd = new SqlCmd(_connection);
				if (!handleException.Entries.Any())
					return _exception;
				DbEntityEntry dbEntityEntry = handleException.Entries.First();
				if ((dbEntityEntry.Entity as IBaseEntity) == null)
					return _exception;
				Entity entity = Objects.ById<Entity>((dbEntityEntry.Entity as IBaseEntity).EntityId);
				string entityType = sqlCmd.ExecuteScalar<string>(string.Format("select dbo.GetCaption({0}, {1})", -2147483613, entity.IdEntityType));
				EntityState state = dbEntityEntry.State;
				switch (innerException.Number)
                {
                    case 1205:
//                    case 52000:
                        {
                            return TransactionDeadlocked.CreateException();
                        }
                    case 50001:
                        {
                            string indexName = Regex.Matches(innerException.Message, "\"([^\"]+)\"")[0].Groups[1].Value;
                            var listFields = sqlCmd.SelectOneColumn<string>(
                                string.Format(
                                    "SELECT [c].[Name] FROM [ref].[Index] [a] INNER JOIN [ml].[Index_EntityField_Indexable] [b] ON [b].[idIndex]=[a].[id] INNER JOIN [ref].[EntityField] [c] ON [c].[id]=[b].[idEntityField] WHERE [a].[idEntity]={0} AND [a].[Name]='{1}'",
                                    entity.Id, indexName));
							DbPropertyValues currentValues = dbEntityEntry.CurrentValues;
                            StringBuilder textMessage = new StringBuilder("Запрещено создавать элементы, пересекающиеся по периоду действия.<br/>Уже есть элементы, действующие в указанный период, с реквизитами: <br/>");
                            foreach (string listField in listFields)
                            {
                                string propertyName =
                                    currentValues.PropertyNames.Single(a => a.Equals(listField, StringComparison.OrdinalIgnoreCase));
                                IEntityField field = entity.Fields.Single(a => a.Name.Equals(listField, StringComparison.OrdinalIgnoreCase));
                                object value;
                                if (field.EntityFieldType == EntityFieldType.Link)
                                    value = sqlCmd.ExecuteScalar<string>(string.Format("select dbo.GetCaption({0}, {1})", field.EntityLink.Id, currentValues[propertyName]));
                                else
                                    value = currentValues[propertyName];
                                textMessage.AppendFormat("'{0}': {1}<br/>", field.Caption, value);
                            }
                            string operation = "";
                            if (state == EntityState.Added)
                                operation = string.Format("Добавление элемента в {0} '{1}'", entityType, entity.Caption);
                            else if (state == EntityState.Modified)
                                operation = string.Format("Изменение элемента в {0} '{1}'", entityType, entity.Caption);
                            string control = string.Format("Нарушение уникальности");
                            return new DalSqlException(operation, control, textMessage.ToString());
                        }
                    case 50002:
                        {
                            string operation = "";
                            if (state == EntityState.Added)
                                operation = string.Format("Добавление элемента в {0} '{1}'", entityType, entity.Caption);
                            else if (state == EntityState.Modified)
                                operation = string.Format("Изменение элемента в {0} '{1}'", entityType, entity.Caption);
                            string control = string.Format("Нарушение корректности вводимых данных");
                            return new DalSqlException(operation, control, "'Дата начала действия' должна быть меньше 'Даты окончания действия'");
                        }
					case 50003:
		                {
							string operation = "";
							if (state == EntityState.Added)
								operation = string.Format("Добавление элемента в {0} '{1}'", entityType, entity.Caption);
							else if (state == EntityState.Modified)
								operation = string.Format("Изменение элемента в {0} '{1}'", entityType, entity.Caption);
							string control = string.Format("Нарушение корректности вводимых данных");
							return new DalSqlException(operation, control, "Не указана сущность для поля общей ссылки");
		                }
					case 50004:
		                {
							if (state == EntityState.Deleted)
							{
								DbPropertyValues currentValues = dbEntityEntry.State == EntityState.Added
																	 ? dbEntityEntry.CurrentValues
																	 : dbEntityEntry.OriginalValues;
								string propertyId =
									currentValues.PropertyNames.SingleOrDefault(a => a.Equals("id", StringComparison.OrdinalIgnoreCase));
								if (string.IsNullOrEmpty(propertyId))
									return _exception;

								int id = (int)currentValues[propertyId];
								List<ResultForGenericLinks> listLinks = sqlCmd.Select<ResultForGenericLinks>(
									string.Format("select c.Caption as [EntityType], b.Name as EntityName, b.Caption as EntityCaption," +
									              "b.[idEntityType] as idEntityType, dbo.GetCaption(a.idReferencesEntity, idReferences) as ElementCaption," +
									              "f.Caption as [HeadEntityType], e.Name as HeadEntityName, e.Caption as HeadEntityCaption," +
									              "e.[idEntityType] as HeadidEntityType " +
									              "from dbo.GenericLinks a " +
									              "inner join ref.Entity b on b.id=a.idReferencesEntity " +
									              "inner join enm.EntityType c on c.id=b.idEntityType " +
									              "left outer join ref.EntityField d on d.[idEntity]=a.idReferencesEntity AND d.idEntityFieldType=7 AND d.Name='idOwner' " +
									              "left outer join ref.Entity e on e.id=d.[idEntityLink] " +
									              "left outer join enm.EntityType f on f.id=e.idEntityType " +
												  "where a.idReferenced={0} AND a.idReferencedEntity={1} order by a.idReferencesEntity", id, entity.Id));
								string currentEntity = "";
								StringBuilder textMessage = new StringBuilder("Имеются ссылки на удаляемый элемент:</br>");
								foreach (ResultForGenericLinks resultForGenericLinkse in listLinks)
								{
									if (string.IsNullOrWhiteSpace(currentEntity) || !currentEntity.Equals(resultForGenericLinkse.EntityName, StringComparison.OrdinalIgnoreCase))
									{
										currentEntity = resultForGenericLinkse.EntityName;
										textMessage.AppendFormat("{0} {1}:</br>", resultForGenericLinkse.EntityType,
															 resultForGenericLinkse.EntityCaption);
										textMessage.AppendFormat("    {0},</br>", resultForGenericLinkse.ElementCaption);
									} else
									{
										textMessage.AppendFormat("    {0},</br>", resultForGenericLinkse.ElementCaption);
									}
								}

								string operation = string.Format("Удаление элемента в {0} '{1}'", entityType, entity.Caption);
								string control = string.Format("Нарушение корректности данных");
								return new DalSqlException(operation, control, textMessage.ToString());
							} else
							{
								throw _exception;
							}
		                }
					case 8152:
		                {
							string operation = "";
							string control = string.Format("Нарушение корректности водимых данных");
							if (state == EntityState.Added)
								operation = string.Format("Добавление элемента в {0} '{1}'", entityType, entity.Caption);
							else if (state == EntityState.Modified)
								operation = string.Format("Изменение элемента в {0} '{1}'", entityType, entity.Caption);
							return new DalSqlException(operation, control, "В одном из полей привышено количество допустимых символов");
						}
                    case 547:
                        {
                            DbPropertyValues currentValues = dbEntityEntry.State == EntityState.Added
																 ? dbEntityEntry.CurrentValues
																 : dbEntityEntry.OriginalValues;
                            string propertyId =
                                currentValues.PropertyNames.SingleOrDefault(a => a.Equals("id", StringComparison.OrdinalIgnoreCase));
                            if (string.IsNullOrEmpty(propertyId))
                                return _exception;

                            int id = (int)currentValues[propertyId];
                            
							if (innerException.Message.Contains("DELETE"))
                            {
                                string tableName = Regex.Matches(innerException.Message, "\"([^\"]+)\"")[2].Groups[1].Value;
                                string fieldName = Regex.Matches(innerException.Message, "\'([^\']+)+\'")[0].Groups[1].Value;
								string captionField =
                                    sqlCmd.ExecuteScalar<string>(
										"SELECT TOP(1) CASE WHEN [b].[idEntityType]=" + (byte)EntityType.Multilink + " THEN 'multilink' ELSE [a].[Name] END FROM [ref].[EntityField] a INNER JOIN [ref].[Entity] b ON [b].[id]=[a].[idEntity] AND [b].[Name]='" +
                                        tableName.Split(new char[] { '.' })[1] + "' where [a].[isCaption]=1 OR [b].[idEntityType]=" + (byte)EntityType.Multilink );
                                string name =
                                    sqlCmd.ExecuteScalar<string>("SELECT Caption FROM [ref].[Entity] WHERE Name='" +
                                                                 tableName.Split(new[] { '.' })[1] + "'");
                                if (string.IsNullOrEmpty(captionField))
                                    captionField = "id";
	                            List<string> listItems;
								if (captionField.Equals("multilink"))
								{
									string rightFieldName = sqlCmd.ExecuteScalar<string>(
										string.Format(
											"select b.Name from ref.Entity a inner join ref.EntityField b on b.[idEntity]=a.[id] AND [b].[idEntityFieldType]={0} AND [b].[Name]<>'{1}' WHERE a.Name='{2}'",
											(byte) EntityFieldType.Link, fieldName, tableName.Split(new char[] {'.'})[1]));

									listItems = sqlCmd.SelectOneColumn<string>(string.Format(
											"select dbo.GetCaption([c].[idEntityLink], [a].[{5}])+' ('+CAST([a].[{5}] as nvarchar)+')' FROM [{0}] a, [ref].[Entity] b INNER JOIN ref.EntityField c ON c.[idEntity]=b.[id] and c.idEntityFieldType={4} AND c.Name<>'{1}' WHERE [b].[Name]='{3}' AND [{1}]={2}",
											tableName.Replace(".", "].["), fieldName, id, tableName.Split(new char[] { '.' })[1], (byte)EntityFieldType.Link, rightFieldName));

								}
								else
								{
									listItems = sqlCmd.SelectOneColumn<string>(
										string.Format(
											"select dbo.GetCaption([b].[id], [a].[id])+' ('+CAST([a].[id] as nvarchar)+')' FROM [{0}] a, [ref].[Entity] b WHERE [b].[Name]='{3}' AND [{1}]={2}",
											tableName.Replace(".", "].["), fieldName, id, tableName.Split(new char[] {'.'})[1]));
								}
	                            StringBuilder textMessage = new StringBuilder();
	                            textMessage.AppendFormat(
		                            captionField.Equals("multilink")
			                            ? "На удаляемый элемент ссылаются следующие строки из Мультилинка '{0}':<br/>"
			                            : "На удаляемый элемент ссылаются следующие строки из таблицы '{0}':<br/>", name);
	                            foreach (string listItem in listItems)
                                {
                                    textMessage.AppendFormat("{0} <br/>", listItem);
                                }
                                string operation = string.Format("Удаление элемента из сущности '{0}'", entity.Caption);
                                const string control = "Нарушение ссылочной целостности данных";
                                return new DalSqlException(operation, control, textMessage.ToString());
                            }
                            if (innerException.Message.Contains("INSERT"))
                            {
                                MatchCollection matches = Regex.Matches(innerException.Message, "\"([^\"]+)\"");
                                string fieldName = matches[0].Groups[1].Value.Split(new char[] { '_' })[1];
                                string captionName =
                                    entity.Fields.Single(a => a.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)).Caption;

                                StringBuilder textMessage = new StringBuilder();
                                string operation = string.Format("Добавление элемента в {0} '{1}'", entityType, entity.Caption);
                                textMessage.AppendFormat("В поле '{0}' указано неверное значение", captionName);
                                string control = string.Format("Нарушение корректности вводимых данных");
                                return new DalSqlException(operation, control, textMessage.ToString());
                            }
                            break;
                        }
                    case 2601:
                        {
                            MatchCollection matchCollection = Regex.Matches(innerException.Message, "\'([^\']+)+\'");

                            string tableName = matchCollection[0].Groups[1].Value;
                            string indexName = matchCollection[1].Groups[1].Value;
                            DbPropertyValues currentValues = dbEntityEntry.CurrentValues;
                            List<string> listFields = sqlCmd.SelectOneColumn<string>(
                                string.Format(
                                    "SELECT d.name FROM sys.indexes a inner join sys.tables b on b.object_id=a.object_id and b.name='{0}' " +
                                    "inner join sys.index_columns c on c.object_id=a.object_id and c.index_id=a.index_id " +
                                    "inner join sys.columns d on d.object_id=a.object_id and d.column_id=c.column_id " +
                                    "where a.name='{1}' order by c.key_ordinal", tableName.Split(new[] { '.' })[1], indexName));
                            StringBuilder textMessage = new StringBuilder(); //new StringBuilder("В справочнике уже присутствует элемент со значениями:<br/>");
                            textMessage.AppendFormat("В {0} '{1}' уже присутствует элемент со значениями:<br/>", entityType, entity.Caption);
                            foreach (string listField in listFields)
                            {
                                string propertyName =
                                    currentValues.PropertyNames.Single(a => a.Equals(listField, StringComparison.OrdinalIgnoreCase));
                                IEntityField field = entity.Fields.Single(a => a.Name.Equals(listField, StringComparison.OrdinalIgnoreCase));
                                object value;
                                if (field.EntityFieldType == EntityFieldType.Link)
                                    value = currentValues[propertyName] == null
                                                ? ""
                                                : sqlCmd.ExecuteScalar<string>(string.Format("select dbo.GetCaption({0}, {1})", field.EntityLink.Id,
                                                                                             currentValues[propertyName]));
                                else
                                    value = currentValues[propertyName];
                                value = (value is bool)
                                            ? ((bool) value) ? "Флаг установлен" : "Флаг не установлен"
                                            : value;
                                textMessage.AppendFormat("'{0}': {1}<br/>", field.Caption, value);
                            }
                            string operation = "";
                            if (state == EntityState.Added)
                                operation = string.Format("Добавление элемента в {0} '{1}'", entityType, entity.Caption);
                            else if (state == EntityState.Modified)
                                operation = string.Format("Изменение элемента в {0} '{1}'", entityType, entity.Caption);
                            const string control = "Нарушение уникальности.";
                            return new DalSqlException(operation, control, textMessage.ToString());
                        }
                    default:
                        return handleException;
                }

            }
            return _exception;
        }


        public SqlExceptionHandler(DbUpdateException exception)
        {
            _exception = exception;
        }

        private SqlException _getInnerSqlException(Exception exception)
        {
            if (exception is SqlException)
            {
                return (exception as SqlException);
            }
            else if (exception.InnerException == null)
            {
                return null;
            }
            else
            {
                return _getInnerSqlException(exception.InnerException);
            }
        }

		private class ResultForGenericLinks
		{
			public string EntityType { get; set; }
			public string EntityName { get; set; }
			public string EntityCaption { get; set; }
			public byte IdEntityType { get; set; }
			public string ElementCaption { get; set; }
			public string HeadEntityType { get; set; }
			public string HeadEntityName { get; set; }
			public string HeadEntityCaption { get; set; }
			public byte HeadIdEntityType { get; set; }
		}

    }
}
