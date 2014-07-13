using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Decorators.EventArguments;
using Platform.Dal.Interfaces;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Декоратор, показывающий наименования для присоединенных полей 
	/// </summary>
	public class AddJoinedFields : SelectDecoratorBase
	{
		/// <summary>
		/// Список обрабатываемых присоединенных полей
		/// </summary>
		private List<IEntityField> _joinedFields;

		/// <summary>
		/// Алиас таблицы
		/// </summary>
		private string _aliasName;

		private ISelectQueryBuilder _builder;
		
		/// <summary>
		/// Дефолтный конструктор
		/// </summary>
		public AddJoinedFields()
		{
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="aliasName">Алиас таблицы</param>
		/// <param name="fields">Список обрабатываемых полей</param>
		public AddJoinedFields(string aliasName, List<IEntityField> fields)
		{
			_aliasName = aliasName;
			_joinedFields = fields;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		public AddJoinedFields(List<IEntityField> fields)
			: this("", fields)
		{
		}

		#region Implementation of ITSqlStatementDecorator

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override  TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null)
				return source;
			
			_prepare();

			var result = source;
			foreach (EntityField joinedField in _joinedFields)
			{
				string[] joinedFields = joinedField.Expression.Split(new char[] {'.'});
				Entity leftEntity = joinedField.Entity;
				IEntityField leftField = leftEntity.Fields.SingleOrDefault(a => a.Name.Equals(joinedFields[0],StringComparison.OrdinalIgnoreCase));
				if (leftField==null || !leftField.IdEntityLink.HasValue)
					continue;
				
				IEntity entity = leftField.EntityLink;
				QualifiedJoinType qualifiedJoinType = leftField.GetQualifiedJoinType();
				string leftTableAlias = _aliasName;
				string rightTableAlias = result.NextAlias();
				joinedFields = joinedFields.Skip(1).ToArray();

				string aliasFieldName = joinedField.Name;
				foreach (string field in joinedFields)
				{
					Expression joinExpression = Helper.CreateBinaryExpression((leftTableAlias + "." + leftField.Name).ToColumn(),
					                                                          (rightTableAlias + ".id").ToColumn(),
					                                                          BinaryExpressionType.Equals);
					result.Join(qualifiedJoinType, Helper.CreateSchemaObjectTableSource(entity.Schema, entity.Name, rightTableAlias),
					            joinExpression);
					leftField = entity.Fields.Single(a => a.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
					if (!leftField.IdEntityLink.HasValue)
					{
						result.AddFields(new List<SelectColumn> { Helper.CreateColumn((rightTableAlias + "." + field).ToColumn(), aliasFieldName) });
						break;
					}
					qualifiedJoinType = qualifiedJoinType == QualifiedJoinType.LeftOuter
						                    ? QualifiedJoinType.LeftOuter
						                    : leftField.GetQualifiedJoinType();
					leftTableAlias = rightTableAlias;
					rightTableAlias = rightTableAlias.GetNextAlias();
					entity = Objects.ById<Entity>(leftField.IdEntityLink.Value);
				}
			}
			//this.Log(result, queryBuilder);
			return result;
		}

		#endregion

		/// <summary>
		/// Подготовка перед выполнением декоратора
		/// </summary>
		private void _prepare()
		{
			if (_aliasName == null)
				_aliasName = _builder.AliasName;
			if (_joinedFields == null)
			{
				List<string> fields = _builder.Fields != null && _builder.Fields.Any()
										  ? _builder.Fields
										  : _builder.Entity.Fields.Cast<EntityField>()
												.Where(isJoined)
												.Select(f => f.Name)
												.ToList();

				_joinedFields = _builder.Entity.Fields.Where(a => fields.Contains(a.Name) && isJoined(a)).ToList();
			} else
			{
				_joinedFields =
					_joinedFields.Where(
						a =>
						a.IdCalculatedFieldType.HasValue && a.CalculatedFieldType == CalculatedFieldType.Joined &&
						!string.IsNullOrEmpty(a.Expression)).ToList();
			}
		}

        private bool isJoined(IEntityField field)
        {
            return field.IdCalculatedFieldType.HasValue
                && field.CalculatedFieldType == CalculatedFieldType.Joined
                && !string.IsNullOrEmpty(field.Expression);
        }
	}
}
