using System;
using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Decorators.EventArguments;
using Platform.Dal.Interfaces;
using Platform.Dal.Requirements;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using System.Linq;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.SqlObjectModel;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Декоратор, добавляющий поля field_Description для ссылочных полей
	/// </summary>
	public class AddDescriptions : SelectDecoratorBase
	{
	    private IEnumerable<IEntityField> DescriptionsFields{ get; set; }
        
        private string _aliasName;

		private ISelectQueryBuilder _builder;

	    private readonly string _descriptionFieldPostfix;

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

		/// <summary>
		/// Конструктор добавления для указанных ссылочных полей принадлежащих сущности поля Description
		/// </summary>
        public AddDescriptions(IEnumerable<IEntityField> descriptionFields, string descriptionFieldPostfix = "_Description")
            : this(null, descriptionFields, descriptionFieldPostfix)
		{
		}

	    /// <summary>
	    /// Конструктор добавления для указанных ссылочных полей принадлежащих сущности поля Description
	    /// </summary>
	    /// <param name="aliasName"></param>
        /// <param name="descriptionFields"></param>
	    /// <param name="descriptionFieldPostfix"></param>
        public AddDescriptions(string aliasName, IEnumerable<IEntityField> descriptionFields, string descriptionFieldPostfix = "_Description")
		{
			_aliasName = aliasName;
            DescriptionsFields = descriptionFields;
	        _descriptionFieldPostfix = descriptionFieldPostfix;
		}

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="descriptionFieldPostfix"></param>
        public AddDescriptions(string descriptionFieldPostfix = "_Description")
		{
            _descriptionFieldPostfix = descriptionFieldPostfix;
		}

        public string DescriptionFieldNameForLink(string fieldName)
        {
            return fieldName + _descriptionFieldPostfix;
        }

        private string DescriptionFieldName(IEntityField field)
        {
            if (field.IsFieldWithForeignKey() || _genericLink.Contains(field.EntityFieldType))
                return DescriptionFieldNameForLink(field.Name);
            
            return field.Name + "_description";
        }

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns></returns>
		protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);
			
			_prepare();

		    var result = source;
			foreach (IEntityField field in DescriptionsFields)
			{
			    if (field.IsFieldWithForeignKey())
			    {
			        _recursiveDescription(result, _aliasName, field, DescriptionFieldName(field), field.GetQualifiedJoinType());
			    }
			    else
			    {
			        Expression descriptionExpression = Helper.CreateFunctionCall("[dbo].[GetCaption]", new List<Expression>
			            {
			                (_aliasName + "." + field.Name + "Entity").ToColumn(),
			                (_aliasName + "." + field.Name).ToColumn()
			            });

			        result.AddFields(new List<SelectColumn>
			            {
			                Helper.CreateColumn(descriptionExpression, DescriptionFieldName(field))
			            });
			    }
			}

			////this.Log(result, queryBuilder);
			return result;
		}

	    /// <summary>
	    /// Рекурсивный метод добавляющий JOIN для доступа к полю содержащему наменование
	    /// </summary>
	    /// <param name="selectStatement">Расширяемое выражение</param>
	    /// <param name="aliasName">Алиас таблицы</param>
	    /// <param name="entityField">Поле, для которого добавляется Description</param>
	    /// <param name="aliasDescriptionField"></param>
	    /// <param name="qualifiedJoinType"></param>
        private void _recursiveDescription(SelectStatement selectStatement, string aliasName, IEntityField entityField, string aliasDescriptionField, QualifiedJoinType qualifiedJoinType, bool firstTime = true)
		{
            //Попали случайно :)
			if (!entityField.IdEntityLink.HasValue)
				return;

            //Если у сущности нет поля-наименования нам нечего больше сказать
			Entity entity = Objects.ById<Entity>(entityField.IdEntityLink.Value);

	        var resultField = firstTime ? entity.DescriptionField : entity.CaptionField;
            if (resultField == null)
                return;

            string newAliasName = selectStatement.NextAlias();
		    
            Expression field;
            if (entityField.IdCalculatedFieldType.HasValue && (entityField.CalculatedFieldType == CalculatedFieldType.AppComputed || entityField.CalculatedFieldType == CalculatedFieldType.NumeratorExpression || entityField.CalculatedFieldType == CalculatedFieldType.DbComputedFunction))
                field = selectStatement.GetSelectColumn(entityField.Name);
            else
                field = selectStatement.GetSourceColumn(aliasName, entityField.Name);
			
            if (field != null)
				selectStatement.Join(qualifiedJoinType,
				                         Helper.CreateSchemaObjectTableSource(entity.Schema, entity.Name, newAliasName),
				                         Helper.CreateBinaryExpression(field, (newAliasName+".id").ToColumn(), BinaryExpressionType.Equals));
			else
				return;

            if (_genericLink.Contains(resultField.EntityFieldType))
			{
				selectStatement.Fields(new List<Field>
					{
                        new Field
							{
								Alias = aliasDescriptionField,
								Experssion = Helper.CreateFunctionCall("[dbo].[GetCaption]", new List<Expression>
									{
										(newAliasName + "." + ( resultField.Name) + "Entity").ToColumn(),
										(newAliasName + "." + ( resultField.Name) ).ToColumn()
									})
							}
					});
            }
            else if ( !resultField.IdEntityLink.HasValue)
				selectStatement.Fields(new List<Field>
					{
						new Field {Alias = aliasDescriptionField, Experssion = Helper.CreateColumn(newAliasName, resultField.Name)}
					});
			else
                _recursiveDescription(selectStatement, newAliasName, (EntityField)resultField, aliasDescriptionField, qualifiedJoinType == QualifiedJoinType.LeftOuter ? QualifiedJoinType.LeftOuter : resultField.GetQualifiedJoinType(), false);
		}



	    private void _prepare()
		{
			if (_aliasName == null)
				_aliasName = _builder.AliasName;

		    EntityFieldType[] linkTypes = new[]
				{
					EntityFieldType.Link,
					// общая ссылка:
					EntityFieldType.ReferenceEntity,
					EntityFieldType.TablepartEntity,
					EntityFieldType.DocumentEntity,
					EntityFieldType.ToolEntity
				};

			Predicate<IEntityField> linkFields = f =>
				(linkTypes.Contains(f.EntityFieldType))
				&& !(f.IdCalculatedFieldType.HasValue &&
					(f.CalculatedFieldType == CalculatedFieldType.ClientComputed || f.CalculatedFieldType == CalculatedFieldType.Joined));

			if (DescriptionsFields == null)
			{
                DescriptionsFields = _builder.Entity.Fields.Where(f => linkFields(f));

				if (_builder.Fields != null && _builder.Fields.Any())
                    DescriptionsFields = DescriptionsFields.Where(f => _builder.Fields.Contains(f.Name));
			}
			else
			{
                DescriptionsFields = DescriptionsFields.Where(f => linkFields(f));
			}
			
		}
	}
}
