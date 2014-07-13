using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Декоратор для фильтрации формы выбора полей, организующих иерархию, для недопущения появления циклов в иерарции
	/// </summary>
	public class AddPreventCircles : TSqlStatementDecorator
	{
		private readonly int _idParentEntityField;

		private readonly int _idItem;

		private ISelectQueryBuilder _builder;
		
		/// <summary>
		/// Нельзя без параметров
		/// </summary>
		private AddPreventCircles()
		{
		}
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="idEntityField">Идентифкатор поля сущности</param>
		/// <param name="idItem">Идентифкатор элемента</param>
		public AddPreventCircles(int idEntityField, int idItem)
		{
			_idItem = idItem;
			_idParentEntityField = idEntityField;
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
			SelectStatement result = (source as SelectStatement);
			if (_builder == null || result == null)
				return source;

			IEntityField entityField = _builder.Entity.Fields.SingleOrDefault(a => a.Id == _idParentEntityField && a.IdEntityLink.HasValue && a.IdEntityLink==a.IdEntity);
			if (entityField==null)
				return source;

			FilterConditions filterConditions = new FilterConditions
				{
					Field = "id",
					Not = true,
					Operator = ComparisionOperator.InList,
					Sql = string.Format("SELECT [id] FROM [dbo].[GetChildrens]({0}, {1}, 1)", _idItem, entityField.Id)
				};

			AddWhere addWhere=new AddWhere(filterConditions, LogicOperator.And, true);
			result = (SelectStatement) addWhere.Decorate(result, queryBuilder);
			//this.Log(result, queryBuilder);
			return result;
		}
		#endregion

		private void _prepare()
		{
			
		}
	}
}
