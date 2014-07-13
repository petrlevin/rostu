using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Microsoft.Data.Schema.ScriptDom.Sql;

using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.Dal.Common.Interfaces.QueryBuilders
{
	public interface IQueryBuilder 
	{


		List<TSqlStatementDecorator> QueryDecorators { get; set; }

		List<ITSqlStatementValidator> QueryValidators { get; set; }

		/// <summary>
		/// Форма, для которой создается запрос
		/// </summary>
		/// <remarks>
		/// Любой запрос обязательно отностится к конкретной сущности. 
		/// Если хочется создать произвольный запрос - создайте Точку данных и привяжите запрос к ней.
		/// </remarks>
		IEntity Entity { get; set; }

		/// <summary>
		/// Метод возвращающий выражение в виде объектной модели из пространства Microsoft.Data.Schema.ScriptDom.Sql.
		/// Чтобы получить тект запроса, вызовите метод Render() у возворащенного объекта.
		/// </summary>
		/// <returns></returns>
		TSqlStatement GetTSqlStatement();

		/// <summary>
		/// Получить sql-команду, готовую для выполнения.
		/// Выполняет GetTSqlStatement и к полученному выражения применяет приватные декораторы.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		SqlCommand GetSqlCommand(SqlConnection connection);
	}
}
