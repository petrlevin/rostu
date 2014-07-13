using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Rights.Functional.Decorators
{
	public class AddFilterByStatusForReference : TSqlStatementDecorator ,IApplyForAggregate

	{
		private ISelectQueryBuilder _builder;

		private string _aliasName;

		private IEntity _entity;

		private int _idEntityGenericLink;

		private int _idEntityField;

		/// <summary>
		/// Список типов поля - общих ссылок
		/// </summary>
		private static readonly EntityFieldType[] _genericLink = new[]
				{
					EntityFieldType.ReferenceEntity,
					EntityFieldType.ToolEntity,
					EntityFieldType.TablepartEntity,
					EntityFieldType.DocumentEntity
				};

		#region Implementation of ITSqlStatementDecorator

		public AddFilterByStatusForReference(int idEntityField, int idEntity)
		{
			_idEntityField = idEntityField;
			_idEntityGenericLink = idEntity;
		}

		public AddFilterByStatusForReference(Entity entity)
		{
			_entity = entity;
			_aliasName = "a";
		}
		
		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			if (_entity == null)
			{
                Entity genericEntity = Objects.ById<Entity>(_idEntityGenericLink);
				EntityField entityField = Objects.ById<EntityField>(_idEntityField);

				if (entityField.EntityFieldType == EntityFieldType.Multilink)
				{
					entityField =
						(EntityField)
						entityField.EntityLink.Fields.Single(a => a.IdEntityLink.HasValue && a.IdEntityLink != entityField.IdEntity);
				}
			    if (!_genericLink.Contains(entityField.EntityFieldType) && entityField.EntityLink == null)
			        return source;
                if (!_genericLink.Contains(entityField.EntityFieldType) && (entityField.EntityLink.EntityType != EntityType.Reference || !entityField.EntityLink.IsRefernceWithStatus()))
					return source;
				if (_genericLink.Contains(entityField.EntityFieldType) && !genericEntity.IsRefernceWithStatus())
					return source;
			}
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null && _entity==null)
				return source;
			
			_prepare();

			FilterConditions filterConditions = new FilterConditions();
			filterConditions.Field = _entity.Fields.Single(a => a.IdEntityLink.HasValue && a.EntityLink.Name == "RefStatus").Name;
			filterConditions.Type = LogicOperator.Simple;
			filterConditions.Operator = ComparisionOperator.Equal;
			filterConditions.Value = (byte) RefStatus.Work;
			AddWhere addWhere = new AddWhere(filterConditions, LogicOperator.And, true);
			source = addWhere.Decorate(source, _builder);

			return source;
		}
		#endregion

		private void _prepare()
		{
			if (_entity == null)
			{
				_entity = _builder.Entity;
				_aliasName = _builder.AliasName;
			}
		}
	}
}
