using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Decorators.EventArguments;
using Platform.Dal.Interfaces;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common.Interfaces;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Декоратор, добавляющий сортировку
	/// </summary>
	public class AddOrder : SelectDecoratorBase, IDecoratorListener, IOrdered
	{
		private string _alias;
		private Order _orderList;

		/// <summary>
		/// Построитель запроса, откуда будет взята информация о сортировке
		/// </summary>
		private ISelectQueryBuilder _builder;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="alias">Алиас sql-выражения (или имя таблицы), для которого будет добавлена сортировка</param>
		/// <param name="orderList">Список полей, по которым осуществляется сортировка</param>
		public AddOrder(string alias, Order orderList)
		{
			_alias = alias;
			_orderList = orderList;
		}

		public AddOrder()
		{
		}

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);
			
			prepare();

			if (string.IsNullOrEmpty(_alias))
				throw new Exception("AddOrder: передан пустой _alias");
			var result = source;

			if (_orderList == null || _orderList.Count == 0)
				return source;

			List<ExpressionWithSortOrder> orders = new List<ExpressionWithSortOrder>();
			foreach (KeyValuePair<string, bool> keyValuePair in _orderList)
			{
				orders.Add(new ExpressionWithSortOrder
					{
						Expression = result.GetSelectColumn(keyValuePair.Key), 
                        SortOrder = keyValuePair.Value ? SortOrder.Ascending : SortOrder.Descending
					});
			}

			result = result.OrderBy(orders);
			//this.Log(result, queryBuilder);
			return result;
		}

		private void prepare()
		{
			if (_alias == null)
				_alias = _builder.AliasName;
			if (_orderList == null)
				_orderList = (Order) _builder.Order;
		}

		#region Implementation of IDecoratorListener

		public void OnDecorated(TSqlStatementDecorator sender, EventDatas eventArgs)
		{
			//var tableAliasEventArgs = eventArgs.OfType<TableAliasEventArgs>().LastOrDefault(); // внимание! будет получено имя, переданное в последнем уведомителе
			//if (tableAliasEventArgs != null)
			//{
			//	_alias = tableAliasEventArgs.TableAlias;
			//}
		}

		#endregion

		#region Implementation of IOrdered

		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> Before { get; private set; }

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After { get
		{
			return new List<Type>()
					{
						typeof (AddCaptions),
						typeof (AddDescriptions),
						typeof (AddJoinedFields)
					};
			
		} }

		/// <summary>
		/// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
		/// </summary>
		public Utils.Common.Order WantBe { get; private set; }

		#endregion
	}
}
