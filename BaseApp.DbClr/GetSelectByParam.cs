using System;
using System.Collections.Generic;
using System.Text;
using Platform.DbClr;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr
{
	/// <summary>
	/// Класс для генерации запроса для получения наименований для сущности
	/// </summary>
	public class GetSelectByParam
	{
		/// <summary>
		/// Дефолтный конструктор перекрыт
		/// </summary>
		private GetSelectByParam(){}

		public GetSelectByParam(IEntity entity, string parameterName)
		{
			_entity = entity;
			_parameterName = parameterName.ToLower();
		}

		/// <summary>
		/// Конструктор для всех полей, кроме общей ссылки
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="parameterName">Имя параметра</param>
		/// <param name="entityField">Поле сущности</param>
		public GetSelectByParam(IEntity entity, string parameterName, IEntityField entityField) : this (entity, parameterName)
		{
			_entityField = entityField;
		}

		/// <summary>
		/// Конструктор для полей с типом общая ссылка
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parameterName"></param>
		/// <param name="idEntities"></param>
		public GetSelectByParam(IEntity entity, string parameterName, List<int> idEntities)
			: this(entity, parameterName)
		{
			_idEntities = idEntities;
		}

		/// <summary>
		/// Конструктор для полей с типом общая ссылка
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="parameterName">Имя параметра</param>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="idEntities">Список уникальных идентификаторов сущностей из общей ссылки</param>
		public GetSelectByParam(IEntity entity, string parameterName, IEntityField entityField, List<int> idEntities)
			: this(entity, parameterName ,entityField)
		{
			_idEntities = idEntities;
		}

		private readonly IEntity _entity;

		private readonly IEntityField _entityField = null;

		private readonly string _parameterName;

		private readonly List<int> _idEntities = new List<int>();
		
		/// <summary>
		/// Получение запроса
		/// </summary>
		/// <returns></returns>
		public string GetResult()
		{
			IEntityField paramField = _entityField ?? getFieldByParameter(_entity);
			StringBuilder textCommand = new StringBuilder();
			if (paramField.EntityFieldType == EntityFieldType.String || paramField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand.AppendFormat("SELECT id, {3} as [idEntity], {0} as Caption FROM [{1}].[{2}]", paramField.Name, _entity.Schema, _entity.Name, _entity.Id);
			}
			else if (paramField.EntityFieldType == EntityFieldType.Int || paramField.EntityFieldType == EntityFieldType.TinyInt || paramField.EntityFieldType == EntityFieldType.SmallInt || paramField.EntityFieldType == EntityFieldType.BigInt)
			{
				textCommand.AppendFormat("SELECT id, {3} as [idEntity], CAST({0} AS NVARCHAR) as Caption FROM [{1}].[{2}]", paramField.Name, _entity.Schema, _entity.Name, _entity.Id);
			}
			else if (paramField.EntityFieldType == EntityFieldType.Link)
			{
				textCommand.Append("SELECT a.id, {2} as [idEntity], [{0}].[{1}] FROM ");
				const char alias = 'a';
				textCommand.AppendFormat("[{0}].[{1}] [{2}] ", _entity.Schema, _entity.Name, alias);
				string joinType = paramField.AllowNull ? Helper.LeftOuterJoin : Helper.InnerJoin;
				_reqursiveGetSelect(textCommand, paramField, joinType, alias);
			} else if (paramField.EntityFieldType == EntityFieldType.ReferenceEntity || paramField.EntityFieldType == EntityFieldType.DocumentEntity || paramField.EntityFieldType == EntityFieldType.TablepartEntity || paramField.EntityFieldType == EntityFieldType.ToolEntity)
			{
				textCommand.Append(getCaptionsForGenericLinks(_entity, paramField));
			}
			return textCommand.ToString();
		}

		/// <summary>
		/// Получения наименований для поля с типом "Общая ссылка"
		/// </summary>
		/// <param name="entity">Сущности</param>
		/// <param name="captionField">Поле Caption сущности</param>
		/// <returns></returns>
		private string getCaptionsForGenericLinks(IEntity entity, IEntityField captionField)
		{
			if (_idEntities.Count == 0) return "";
			StringBuilder unionCommand = new StringBuilder();
			foreach (int idEntity in _idEntities)
			{
				if (unionCommand.Length != 0)
					unionCommand.AppendLine("UNION ALL ");
				IEntity internalEntity = Objects.ById<Entity>(idEntity);
				GetSelectByParam getSelectByParam=new GetSelectByParam(internalEntity, "caption");
				unionCommand.AppendLine(getSelectByParam.GetResult());
			}
			return
				unionCommand.ToString();
			/*string.Format(
					"SELECT [Id], {0} as [IdEntity], [b].[Caption] as [Caption] FROM [{1}].[{2}] a INNER JOIN ({3}) b ON [b].[IdEntity]=[a].[{4}Entity] AND [b].[Id]=[a].[{4}]",
					entity.Id, entity.Schema, entity.Name, unionCommand, captionField.Name);*/
		}

		/// <summary>
		/// Рекурсивная функция для построения запроса
		/// </summary>
		/// <param name="textCommand">Изменяемы текст команды</param>
		/// <param name="paramField">Caption поле</param>
		/// <param name="joinType">Тип соединения</param>
		/// <param name="alias">Алиас таблицы</param>
		private void _reqursiveGetSelect(StringBuilder textCommand, IEntityField paramField, string joinType, char alias)
		{
			char nextAlias = alias;
			nextAlias++;
			IEntity nextEntity = paramField.EntityLink;
			IEntityField nextParamField = getFieldByParameter(nextEntity);
			textCommand.AppendFormat("{0} [{1}].[{2}] [{3}] ON [{4}].[{5}]=[{3}].[id] ", joinType, nextEntity.Schema,
									 nextEntity.Name, nextAlias, alias, paramField.Name);
			if (nextParamField.EntityFieldType == EntityFieldType.String || nextParamField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand.Replace("{0}", nextAlias.ToString());
				textCommand.Replace("{1}", nextParamField.Name);
				textCommand.Replace("{2}", _entity.Id.ToString());
			}
			else if (nextParamField.EntityFieldType == EntityFieldType.Link)
			{
				string nextJoinType = joinType == Helper.LeftOuterJoin
										  ? Helper.LeftOuterJoin
										  : (nextParamField.AllowNull ? Helper.LeftOuterJoin : Helper.InnerJoin);
				_reqursiveGetSelect(textCommand, nextParamField, nextJoinType, nextAlias);
			}
			else
			{
				throw new Exception("Не реализовано для " + nextParamField.EntityFieldType);
			}
		}

		/// <summary>
		/// Получение поля по имения параметра
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		private IEntityField getFieldByParameter(IEntity entity)
		{
			switch (_parameterName)
			{
				case "caption":
					return entity.CaptionField;
				case "description":
					return entity.DescriptionField;
				default:
					throw new Exception("Не реализовано для " + _parameterName);
			}
		}

	}
}
