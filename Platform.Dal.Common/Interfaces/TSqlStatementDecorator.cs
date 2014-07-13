using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces.QueryBuilders;

namespace Platform.Dal.Common.Interfaces
{
	/// <summary>
	/// Базовый класс для всех декораторов
	/// </summary>
    public abstract class TSqlStatementDecorator
	{
		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected abstract TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder);
        
        public TSqlStatement Decorate(TSqlStatement source, IQueryBuilder queryBuilder)
        {
            var result = DoDecorate(source, queryBuilder);
            OnDecorated(this, result, queryBuilder);
            return result;
        }

	    public static event DecoratedHandler Decorated;

	    private static void OnDecorated(TSqlStatementDecorator decorator, TSqlStatement source, IQueryBuilder querybuilder)
	    {
	        var handler = Decorated;
	        if (handler != null) handler(decorator, source, querybuilder);
	    }

	    public delegate void DecoratedHandler(TSqlStatementDecorator decorator, TSqlStatement source, IQueryBuilder queryBuilder);
	}
}
