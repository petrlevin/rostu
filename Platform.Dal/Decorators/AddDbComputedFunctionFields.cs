using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;


namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Декоратор добавления полей с типом вычисляемого поля DbComputedFunction в выборку
	/// </summary>
	public class AddDbComputedFunctionFields : SelectDecoratorBase, IOrdered
	{
		private ISelectQueryBuilder _builder;

		private IOrderedEnumerable<IEntityField> _fields;

		#region Overrides of SelectDecoratorBase

		protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);

			_prepare();
			if (!_fields.Any())
				return source;
			string newAlias = "";
			string function = "";
			string functionName = "";
			string functionSchema = "";
			foreach (IEntityField entityField in _fields)
			{
				if (!function.Equals(entityField.Expression, StringComparison.OrdinalIgnoreCase))
				{
					newAlias = source.NextAlias();
					function = entityField.Expression;
					functionSchema = function.Split(new char[] {'.'})[0].Replace("[", "").Replace("]", "");
					functionName = function.Split(new char[] { '.', '(' })[1].Replace("[", "").Replace("]", "");
					SchemaObjectTableSource table = Helper.CreateSchemaObjectTableSource(functionSchema, functionName, newAlias);
					table.ParametersUsed = true;
					BinaryExpression joinExpression = Helper.CreateBinaryExpression((_builder.AliasName + ".id").ToColumn(),
					                                                   (newAlias + ".id").ToColumn(), BinaryExpressionType.Equals);
					source = source.Join(QualifiedJoinType.LeftOuter, table, joinExpression);
				}
                source = source.AddField(Helper.CreateColumn((newAlias + "." + entityField.Name).ToColumn(), entityField.Name));
				/*source = source.AddField(
						Helper.CreateColumn(
							Helper.CreateFunctionCall("ISNULL",
							                          new List<Expression>() {(newAlias + "." + entityField.Name).ToColumn(), 0.ToLiteral()}),
							entityField.Name));*/
			}
			return source;
		}

		#endregion

		private void _prepare()
		{
			List<string> selectFields = _builder.Fields ?? _builder.Entity.Fields.Select(a => a.Name).ToList();
			_fields = _builder.Entity.Fields.ToList().Where(
				a =>
				a.CalculatedFieldType == CalculatedFieldType.DbComputedFunction &&
				selectFields.Any(b => b.Equals(a.Name, StringComparison.OrdinalIgnoreCase))).OrderBy(o => o.Expression);
		}

		#region Implementation of IOrdered

		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> Before
		{
			get
			{
				return new List<Type>()
					{
						typeof (AddCaptions),
						typeof (AddJoinedFields)
					};
			}
		}

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After { get
		{
			return new List<Type>()
					{
						typeof (AddHierarcyFilter),
						typeof (AddActualItemForVersioning)
					};
			
		} }

		/// <summary>
		/// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
		/// </summary>
		public Order WantBe { get
		{
			return Order.Last;
		} }

		#endregion
	}
}
