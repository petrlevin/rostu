using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using BaseApp.Reference;
using BaseApp.Rights;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Controls.DispatcherStrategies;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.DataAccess;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.XmlExchange.Import
{
	public class SimpleImporter
	{
		private int _idEntity;
		
		private XDocument _document;

		private SqlConnection _connection = IoC.Resolve<SqlConnection>("DbConnection");

		private List<InsertedElement> _insertedElements = new List<InsertedElement>();

		private SqlCmd _sqlCmd;

		private Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();

		private Dictionary<string, DataManager> _dataManagers = new Dictionary<string, DataManager>();

		public SimpleImporter(int idEntity, XDocument document)
		{
			_idEntity = idEntity;
			_document = document;
			_sqlCmd = new SqlCmd(_connection);
		}

		public string Execute()
		{
			if (_document == null || _document.Root == null)
				return "Пустой файл";
			Entity entity = Objects.ById<Entity>(_idEntity);
			bool valid = _document.Root.Descendants(string.Format("{0}.{1}", entity.Schema, entity.Name)).Any();
			if (!valid)
			{
				throw new Exception("Нет данных для указанной сущности");
			}
			using (new ControlScope<SkipSkippableStrategy>(()=>true , ScopeOptions.ApplyOnlyDispatching))
			{
				foreach (XElement element in _document.Root.Elements())
				{
					_insertElement(element);
				}
			}
			return "Импорт завершен";
		}

		private void _insertElement(XElement element)// Dictionary<string, object> values, Entity entity, DataManager dataManager)
		{
			Entity entity;
			DataManager dataManager;
			if (!_entities.TryGetValue(element.Name.LocalName, out entity))
			{
				string entityName = element.Name.LocalName.Split(new char[] {'.'})[1];
				entity = Objects.ByName<Entity>(entityName);
				_entities.Add(element.Name.LocalName, entity);
			}
			if (!_dataManagers.TryGetValue(element.Name.LocalName, out dataManager))
			{
				dataManager = DataManagerFactory.Create(entity);
				_dataManagers.Add(element.Name.LocalName, dataManager);
			}
			Dictionary<string, string> stringValues = element.Elements().ToDictionary(a => a.Name.LocalName, b => b.Value);
			Dictionary<string, object> values = stringValues.ConverValues(entity);
			object value = null;
			if (entity.EntityType == EntityType.Multilink)
			{
				_insertElementMl(element);
				return;
			}
			if (!values.TryGetValue("id", out value))
				throw new Exception("Нет поля id");
			if (_insertedElements.Any(a => a.IdEntity == entity.Id && a.OldId == Convert.ToInt32(value)))
				return;

			Dictionary<string, object> valuesForInsert =
				_replaseLinkValues(values, entity);

			if (valuesForInsert != null && !_checkExist(valuesForInsert, entity))
			{
				try
				{
					int? newId =
						dataManager.CreateEntry(valuesForInsert.Where(a => a.Key != "id").ToDictionary(a => a.Key, b => b.Value));
					if (newId != null)
					{
						_insertedElements.Add(new InsertedElement
							{IdEntity = entity.Id, NewId = newId.Value, OldId = Convert.ToInt32(value)});
					}
				} catch(Exception exception)
				{
					if (exception is ControlException)
						throw;
				}
			}
		}

		private void _insertElementMl(XElement element)
		{
			Entity entity;
			DataManager dataManager;
			if (!_entities.TryGetValue(element.Name.LocalName, out entity))
			{
				string entityName = element.Name.LocalName.Split(new char[] { '.' })[1];
				entity = Objects.ByName<Entity>(entityName);
				_entities.Add(element.Name.LocalName, entity);
			}
			if (!_dataManagers.TryGetValue(element.Name.LocalName, out dataManager))
			{
				dataManager = DataManagerFactory.Create(entity);
				_dataManagers.Add(element.Name.LocalName, dataManager);
			}
			Dictionary<string, string> stringValues = element.Elements().ToDictionary(a => a.Name.LocalName, b => b.Value);
			Dictionary<string, object> values = stringValues.ConverValues(entity);
			Dictionary<string, object> valuesForInsert =
				_replaseLinkValues(values.Where(a => a.Key != "id").ToDictionary(a => a.Key, b => b.Value), entity);
			if (valuesForInsert!=null && !_checkExist(valuesForInsert, entity))
			{
				int firstId = 0;
				int firstEntityId = 0;
				int secondId = 0;
				foreach (IEntityField entityField in entity.Fields.Where(a => a.EntityFieldType == EntityFieldType.Link && a.IdEntityLink.HasValue))
				{
					if (firstId == 0)
					{
						firstId = Convert.ToInt32(valuesForInsert[entityField.Name]);
						firstEntityId = entityField.EntityLink.Id;
					}
					else
					{
						secondId = Convert.ToInt32(valuesForInsert[entityField.Name]);
					}
				}
				dataManager.CreateMultilink(firstEntityId, firstId, new int[] { secondId });
			}
		}

		private Dictionary<string, object> _replaseLinkValues(Dictionary<string, object> values, Entity entity)
		{
			List<string> baseEntity = new List<string>
				{
					"Entity",
					"EntityField"
				};
			List<IEntityField> linkedFields =
				entity.Fields.Where(
					a => a.EntityFieldType == EntityFieldType.Link 
						&& !a.IdCalculatedFieldType.HasValue 
						&& a.IdEntityLink.HasValue 
						&& a.EntityLink.EntityType != EntityType.Enum 
						&& !baseEntity.Contains(a.EntityLink.Name, StringComparer.OrdinalIgnoreCase))
						.ToList();
			if (linkedFields.Count==0)
				return values;
			Dictionary<string, object> result = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> keyValuePair in values)
			{
				IEntityField entityField =
					linkedFields.SingleOrDefault(a => a.Name.Equals(keyValuePair.Key, StringComparison.OrdinalIgnoreCase));
				if (entityField==null)
				{
					result.Add(keyValuePair.Key, keyValuePair.Value);
				} else
				{
					int oldValue = Convert.ToInt32(keyValuePair.Value);
					int? newValue = _getFromInserted(oldValue, entityField.EntityLink.Id) ??
									_getFromDocument(oldValue, string.Format("{0}.{1}", entityField.EntityLink.Schema, entityField.EntityLink.Name));
					if (newValue == null)
						return null;
						//throw new Exception("Не удалось вычислить новое значение дал ссылочного поля "+keyValuePair.Key);
					result.Add(keyValuePair.Key, newValue);
				}
			}
			return result;
		}

		private int? _getFromInserted(int id, int idEntity)
		{
			InsertedElement element = _insertedElements.SingleOrDefault(a => a.IdEntity == idEntity && a.OldId == id);
			if (element == null)
				return null;
			else
			{
				return element.NewId;
			}
		}

		private int? _getFromDocument(int id, string nameElement)
		{
			XElement element =
				_document.Root.Descendants(nameElement).FirstOrDefault(b=> b.Elements().Any(a => a.Name.LocalName == "id" && int.Parse(a.Value) == id));
			if (element == null)
				return null;
			_insertElement(element);
			Entity entity = _entities[nameElement];
			return _getFromInserted(id, entity.Id);
		}

		private bool _checkExistMl(Dictionary<string, object> values, Entity entity)
		{
			SelectQueryBuilder queryBuilder = new SelectQueryBuilder(entity);
			FilterConditions filterConditions = new FilterConditions
				{
					Type = LogicOperator.And,
					Operands = new List<IFilterConditions>()
				};
			foreach (IEntityField entityField in entity.Fields.Where(a=> a.EntityFieldType==EntityFieldType.Link && a.IdEntityLink.HasValue))
			{
				object value;
				if (!values.TryGetValue(entityField.Name, out value))
				{
					throw new Exception("Для поля " + entityField.Name + " мультилинка не передано значение");
				} else
				{
					filterConditions.Operands.Add(new FilterConditions
						{
							Field = entityField.Name,
							Value=value,
							Operator = ComparisionOperator.Equal
						});
				}
			}
			queryBuilder.Conditions = filterConditions;
			object dbId;
			using (SqlCommand command = queryBuilder.GetSqlCommand(_connection, new List<TSqlStatementDecorator> { new AddWhere() }))
			{
				dbId = command.ExecuteScalar();
			}
			return dbId != null;
		}

		private bool _checkExistTp(Dictionary<string, object> values, Entity entity)
		{
			SelectQueryBuilder queryBuilder = new SelectQueryBuilder(entity, new List<string> { "id" });
			IEntityField captionField = entity.CaptionField;
			IEntityField ownerField =
				entity.Fields.SingleOrDefault(a => a.Name.Equals("idowner", StringComparison.OrdinalIgnoreCase));
			if (ownerField == null)
				throw new Exception("У ТЧ не найдено поле idOwner");
			object value;
			object valueOwner;
			bool result = false;
			if (values.TryGetValue(captionField.Name, out value) && values.TryGetValue(ownerField.Name, out valueOwner))
			{
				FilterConditions filterConditions = new FilterConditions
				{
					Type = LogicOperator.And,
					Operands = new List<IFilterConditions>()
				};
				filterConditions.Operands.Add(new FilterConditions
					{
						Field = captionField.Name,
						Value = value,
						Operator = ComparisionOperator.Equal
					});
				filterConditions.Operands.Add(new FilterConditions
				{
					Field = ownerField.Name,
					Value = valueOwner,
					Operator = ComparisionOperator.Equal
				});
				object dbId;
				//queryBuilder.Conditions = filterConditions;
				using (SqlCommand command = queryBuilder.GetSqlCommand(_connection, new List<TSqlStatementDecorator> { new AddSysDimensionsFilter(null), new AddWhere(filterConditions, LogicOperator.And) }))
				{
					dbId = command.ExecuteScalar();
				}
				result = dbId != null;
			}
			return result;
		}

		private bool _checkExist(Dictionary<string, object> values, Entity entity)
		{
			if (entity.EntityType == EntityType.Multilink) return _checkExistMl(values, entity);
			if (entity.EntityType == EntityType.Tablepart) return _checkExistTp(values, entity);

			SelectQueryBuilder queryBuilder = new SelectQueryBuilder(entity, new List<string> {"id"});
			IEntityField captionField = entity.CaptionField;
			object value;
			bool result = false;
			if (values.TryGetValue(captionField.Name, out value))
			{
				/*queryBuilder.Conditions = new FilterConditions
					{
						Field = captionField.Name,
						Value = value,
						Operator = ComparisionOperator.Equal
					};*/
				int? dbId;
				using (SqlCommand command = queryBuilder.GetSqlCommand(_connection, new List<TSqlStatementDecorator> {new AddSysDimensionsFilter(null), new AddWhere(new FilterConditions
					{
						Field = captionField.Name,
						Value = value,
						Operator = ComparisionOperator.Equal
					}, LogicOperator.And)}))
				{
					dbId = (int?)command.ExecuteScalar();
				}
				if (dbId!=null)
				{
					values.TryGetValue("id", out value);
					result = true;
					_insertedElements.Add(new InsertedElement {IdEntity = entity.Id, NewId = dbId.Value, OldId = Convert.ToInt32(value)});
				}
			} else
			{
				throw new Exception("Поле Caption не заполнено");
			}
			return result;
		}

		private class InsertedElement
		{
			public int IdEntity;
			public int OldId;
			public int NewId;
		}
		
	}
	
	internal static class Extensions
	{
		public static Dictionary<string, object> ConverValues(this Dictionary<string, string> values, Entity entity)
		{
			Dictionary<string, object> result=new Dictionary<string, object>();
			foreach (KeyValuePair<string, string> keyValuePair in values)
			{
				IEntityField entityField =
					entity.Fields.SingleOrDefault(a => a.Name.Equals(keyValuePair.Key, StringComparison.OrdinalIgnoreCase));
				if (entityField == null)
					throw new Exception("Не надено поле " + keyValuePair.Key + " в сущности " + entity.Name);
				result.Add(keyValuePair.Key, keyValuePair.Value.GetObjectValue(entityField));
			}
			return result;
		}
		public static object GetObjectValue(this string value, IEntityField entityField)
		{
			switch (entityField.EntityFieldType)
			{
				
				case EntityFieldType.Bool:
					return (value == "1" || value == "true");
				case EntityFieldType.Int:
					return Convert.ToInt32(value);
				case EntityFieldType.SmallInt:
					return Convert.ToInt16(value);
				case EntityFieldType.TinyInt:
					return Convert.ToByte(value);
				case EntityFieldType.BigInt:
					return Convert.ToInt64(value);
				case EntityFieldType.Date:
					return Convert.ToDateTime(value).Date;
				case EntityFieldType.DateTime:
					return Convert.ToDateTime(value);
				case EntityFieldType.Text:
				case EntityFieldType.String:
                	return value;
                case EntityFieldType.File:
			        return "Файл";
				case EntityFieldType.Link:
				case EntityFieldType.DocumentEntity:
				case EntityFieldType.ReferenceEntity:
				case EntityFieldType.TablepartEntity:
				case EntityFieldType.ToolEntity:
					return int.Parse(value);
				case EntityFieldType.Numeric:
                case EntityFieldType.Money:
					return decimal.Parse(value.Replace('.',','));
                default:
					throw new Exception("Не реализовано");
			}
		}
	}

}