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
	/// Декоратор, добавляющий поля field_Caption для ссылочных полей
	/// </summary>
	public class AddCaptions : SelectDecoratorBase
	{
	    private IEnumerable<IEntityField> __captionFields;
        private IEnumerable<IEntityField> _captionFields 
        {
            get { return __captionFields; }
            set 
            { 
                __captionFields = value;
            }
        }

        private string _aliasName { get; set; }

        private ISelectQueryBuilder _builder { get; set; }

        private readonly string _captionFieldPostfix;

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
		/// Конструктор добавления для указанных ссылочных полей принадлежащих сущности поля Caption
		/// </summary>
        public AddCaptions(IEnumerable<IEntityField> captionFields, string captionFieldPostfix = "_Caption")
            : this(null, captionFields, captionFieldPostfix)
		{
		}

	    /// <summary>
	    /// Конструктор добавления для указанных ссылочных полей принадлежащих сущности поля Caption
	    /// </summary>
	    /// <param name="aliasName"></param>
	    /// <param name="captionFields"></param>
	    /// <param name="captionFieldPostfix"></param>
	    public AddCaptions(string aliasName, IEnumerable<IEntityField> captionFields,string captionFieldPostfix="_Caption")
		{
			_aliasName = aliasName;
			_captionFields = captionFields;
		    _captionFieldPostfix = captionFieldPostfix;
		}

        public AddCaptions(string captionFieldPostfix = "_Caption")
		{
            _captionFieldPostfix = captionFieldPostfix;
		}


        public string CaptionFieldNameForLink(string fieldName)
        {
            return fieldName + _captionFieldPostfix;
        }

        private string CaptionFieldName(IEntityField field)
        {
            if (field.IsFieldWithForeignKey() || _genericLink.Contains(field.EntityFieldType))
                return CaptionFieldNameForLink(field.Name);
            
            return field.Name + "_caption";
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
			foreach (IEntityField captionField in _captionFields)
			{
				if (captionField.IsFieldWithForeignKey())
				{
                    _recursiveCaption(result, _aliasName, captionField, CaptionFieldName(captionField),
					                  captionField.GetQualifiedJoinType());
				}
				else
				{
					Expression expression = Helper.CreateFunctionCall("[dbo].[GetCaption]", new List<Expression>
						{
							(_aliasName + "." + captionField.Name + "Entity").ToColumn(),
							(_aliasName + "." + captionField.Name).ToColumn()
						});
					result.AddFields(new List<SelectColumn> {Helper.CreateColumn(expression, CaptionFieldName(captionField))});
				}
			}
			return result;
		}

		/// <summary>
		/// Рекурсивный метод добавляющий JOIN для доступа к полю содержащему наменование
		/// </summary>
		/// <param name="selectStatement">Расширяемое выражение</param>
		/// <param name="aliasName">Алиас таблицы</param>
		/// <param name="entityField">Поле, для которого добавляется Caption</param>
		/// <param name="aliasField">Алиас поля, для которого добавляется Caption</param>
		private void _recursiveCaption(SelectStatement selectStatement, string aliasName, IEntityField entityField, string aliasField, QualifiedJoinType qualifiedJoinType)
		{
			if (!entityField.IdEntityLink.HasValue)
				return;

			Entity entity = Objects.ById<Entity>(entityField.IdEntityLink.Value);
			string newAliasName = selectStatement.NextAlias();
		    Expression field;
            if (entityField.IdCalculatedFieldType.HasValue && (entityField.CalculatedFieldType == CalculatedFieldType.AppComputed || entityField.CalculatedFieldType == CalculatedFieldType.NumeratorExpression || entityField.CalculatedFieldType == CalculatedFieldType.DbComputedFunction))
                field = selectStatement.GetSelectColumn(entityField.Name);
            else
                field = selectStatement.GetSourceColumn(aliasName, entityField.Name);
			if (field != null)
			{
				selectStatement.Join(qualifiedJoinType,
				                     Helper.CreateSchemaObjectTableSource(entity.Schema, entity.Name, newAliasName),
				                     Helper.CreateBinaryExpression(field, (newAliasName+".id").ToColumn(), BinaryExpressionType.Equals));
			}
			else
			{
				return;

			}
			if (_genericLink.Contains(entity.CaptionField.EntityFieldType))
			{
				selectStatement.Fields(new List<Field>
					{
						new Field
							{
								Alias = aliasField,
								Experssion = Helper.CreateFunctionCall("[dbo].[GetCaption]", new List<Expression>
									{
										(newAliasName + "." + entity.CaptionField.Name + "Entity").ToColumn(),
										(newAliasName + "." + entity.CaptionField.Name).ToColumn()
									})
							}
					});
			} else if (!entity.CaptionField.IdEntityLink.HasValue)
				selectStatement.Fields(new List<Field>
					{
						new Field {Alias = aliasField, Experssion = Helper.CreateColumn(newAliasName, entity.CaptionField.Name)}
					});
			else
				_recursiveCaption(selectStatement, newAliasName, (EntityField)entity.CaptionField, aliasField, qualifiedJoinType == QualifiedJoinType.LeftOuter ? QualifiedJoinType.LeftOuter : entity.CaptionField.GetQualifiedJoinType());
		}



	    private void _prepare()
		{
			if (_aliasName == null)
				_aliasName = _builder.AliasName;

		    EntityFieldType[] linkTypes = new[]
				{
					EntityFieldType.Link,
                    EntityFieldType.FileLink, 
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

			if (_captionFields == null)
			{
				_captionFields = _builder.Entity.Fields.Where(f => linkFields(f));

				if (_builder.Fields != null && _builder.Fields.Any())
					_captionFields = _captionFields.Where(f => _builder.Fields.Contains(f.Name));
			}
			else
			{
				_captionFields = _captionFields.Where(f => linkFields(f));
			}
			
		}
	}
}
