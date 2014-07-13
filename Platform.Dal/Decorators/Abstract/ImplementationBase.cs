using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces.QueryBuilders;

namespace Platform.Dal.Decorators.Abstract
{
    public abstract class ImplementationBase<TTSqlStatement,TQueryBuilder> where TTSqlStatement : TSqlStatement  where TQueryBuilder :IQueryBuilder
    {

        public TTSqlStatement Source { get; set; }
        public TQueryBuilder QueryBuilder { get; set; }

        public abstract TSqlStatement Decorate();
    }
}
