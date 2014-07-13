using System;
using System.Linq;
using System.Reflection;
using BaseApp.DbEnums;
using BaseApp.Numerators;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.DataAccess;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel.Extensions;
using Helper = Platform.SqlObjectModel.Helper;

namespace BaseApp.Rights
{
    public class AddSysDimensionsFilter : TSqlStatementDecorator, IApplyForAggregate
	{
		private ISelectQueryBuilder _builder;
	    private int? _idEntityField;
		private AddSysDimensionsFilter()
		{
		}

		public AddSysDimensionsFilter(int? idEntityField)
		{
			_idEntityField = idEntityField;
		}

		#region Implementation of ITSqlStatementDecorator

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null)
				return source;
			
			SelectStatement result = (source as SelectStatement);
			Expression whereExpression = null;
			foreach (string item in Enum.GetNames(typeof(SysDimension)))
			{
				if (_idEntityField.HasValue && GetEntityFieldSettingForSysDimensionFilter.Get(_idEntityField.Value, item))
					continue;

				IEntityField entityField =
					_builder.Entity.Fields.SingleOrDefault(
						a => a.FieldDefaultValueType == FieldDefaultValueType.Application && a.DefaultValue.Equals("{id" + item + "}", StringComparison.OrdinalIgnoreCase));
				if (entityField != null)
				{
					BaseAppNumerators baseAppNumerators = new BaseAppNumerators();
					Type type = typeof (BaseAppNumerators);
					MethodInfo method = type.GetMethod("Id" + item);
					object value = method.Invoke(baseAppNumerators, null);
					if (Convert.ToInt32(value) != 0)
					{
						whereExpression = whereExpression.AddExpression(
							Helper.CreateBinaryExpression((_builder.AliasName + "." + entityField.Name).ToColumn(), value.ToLiteral(),
							                              BinaryExpressionType.Equals), BinaryExpressionType.And);
					}
				}
			}
			if (whereExpression!=null)
			{
				(result.QueryExpression as QuerySpecification).AddWhere(BinaryExpressionType.And,
				                                                        whereExpression.ToParenthesisExpression());
			}
			//this.Log(result, queryBuilder);
			return result;
		}

		#endregion
	}
}
