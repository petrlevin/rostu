using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.DataAccess
{
	/// <summary>
	/// Класс описывающий получение фильтра
	/// </summary>
	public static class GetFilter
	{
		/// <summary>
		/// Соединение с БД
		/// </summary>
		private static readonly SqlConnection _connection = IoC.Resolve<SqlConnection>("DbConnection");

	    /// <summary>
	    /// Сущность Filter
	    /// </summary>
	    //private static readonly Entity _entity = Objects.ByName<Entity>("Filter");
        private static readonly Entity _entity = Objects.ByName<Entity>("Filter");

	    /// <summary>
	    /// Получение экземпляра Filter
	    /// </summary>
	    /// <param name="idEntityField">Поле для которого получается фильтр</param>
	    /// <param name="onlyEnabled"></param>
	    /// <returns></returns>
	    public static Filter Get(int idEntityField, bool onlyEnabled = true)
		{
			Filter result = new Filter();
		    var queryBuilder = new SelectQueryBuilder(_entity);
		    
		        FilterConditions filterConditions = new FilterConditions
		                                                {
		                                                    Type = LogicOperator.And,
		                                                    Operands = new List<IFilterConditions>
		                                                                   {
		                                                                       new FilterConditions
		                                                                           {
		                                                                               Field = "idEntityField",
		                                                                               Type =
		                                                                                   LogicOperator.Simple,
		                                                                               Operator =
		                                                                                   ComparisionOperator
		                                                                                   .Equal,
		                                                                               Value = idEntityField
		                                                                           },
		                                                                       //new FilterConditions {Field = "idParent", Type = LogicOperator.Simple, Operator = ComparisionOperator.IsNull}
		                                                                   }

		                                                };
		        if (onlyEnabled)
		            filterConditions.Operands.Add(new FilterConditions()
		                                              {
		                                                  Field = "Disabled",
		                                                  Type = LogicOperator.Simple,
		                                                  Operator = ComparisionOperator.Equal,
		                                                  Value = 0
		                                              });
		        AddWhere addWhere = new AddWhere(filterConditions);
		        SqlCommand command = queryBuilder.GetSqlCommand(_connection, new List<TSqlStatementDecorator> {addWhere});
		        SqlDataAdapter adapter = new SqlDataAdapter(command);
		        DataTable table = new DataTable();
		        adapter.Fill(table);

		    if (!table.Columns.Contains("idParent"))
		    {
		        queryBuilder.Log();
		        throw new PlatformException("При получении параметров фильтра произошла ошибка. Имя сущности фильтра " + _entity.Name + ". " +
		                                    "Поле на которое накладывался фильтр: #" + idEntityField + " " + DateTime.Now);
		    }

		    if (table.Rows.Cast<DataRow>().All(a => a.Field<int?>("idParent").HasValue))
		            return null;
                result.FromDataRow(table.Rows.Cast<DataRow>().First(a => !a.Field<int?>("idParent").HasValue));

		        result.ChildFilter = new List<Filter>();
		        result.ChildFilter.AddRange(GetChilds(result.Id, table));
		        return result;
		    
		}

		/// <summary>
		/// Получение подчиненных частей фильтра
		/// </summary>
		/// <param name="idParent">Идентификатор родителя</param>
		/// <returns></returns>
        private static IEnumerable<Filter> GetChilds(int idParent, DataTable table, bool onlyEnabled = true)
		{
			List<Filter> result=new List<Filter>();
			Predicate<DataRow> ids = row => row.Field<int?>("idParent").HasValue && row.Field<int?>("idParent") == idParent;
			//FilterConditions filterConditions = new FilterConditions { Field = "idParent", Type = LogicOperator.Simple, Operator = ComparisionOperator.Equal, Value = idParent };
		    if (onlyEnabled)
		    {
			    ids =
				    row =>
				    row.Field<int?>("idParent").HasValue && row.Field<int?>("idParent") == idParent &&
				    row.Field<bool>("Disabled") == false;
			    /*filterConditions = new FilterConditions
		                               {
		                                   Type = LogicOperator.And,
		                                   Operands = new List<IFilterConditions>
		                                                  {
		                                                      filterConditions,
		                                                      new FilterConditions()
		                                                          {
		                                                              Field = "Disabled",
		                                                              Type = LogicOperator.Simple,
		                                                              Operator = ComparisionOperator.Equal,
		                                                              Value = 0
		                                                          }
		                                                  }
		                               };*/
		    }

		    //AddWhere addWhere = new AddWhere(filterConditions);
			//SelectQueryBuilder queryBuilder = new SelectQueryBuilder(_entity);
			//queryBuilder.QueryDecorators.Add(addWhere);
			//SqlCommand command = queryBuilder.GetSqlCommand(_connection);
			//SqlDataAdapter adapter = new SqlDataAdapter(command);
			//DataTable table = new DataTable();
			//adapter.Fill(table);
			foreach (DataRow row in table.Rows.Cast<DataRow>().Where(a=> ids(a)))
			{
				Filter filter=new Filter();
				filter.FromDataRow(row);
				filter.ChildFilter = new List<Filter>();
				filter.ChildFilter.AddRange(GetChilds(filter.Id, table));
				result.Add(filter);
			}
			return result;
		}
	}
}
