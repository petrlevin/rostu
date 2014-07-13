using System;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;

namespace Platform.Dal.Decorators.Abstract
{
    public abstract class SelectDecoratorBase : TSqlStatementDecorator
    {
        protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var select = source as SelectStatement;
            if (select==null)
                throw new PlatformException(String.Format("Декоратор {0} можно использовать только для выборки данных.", GetType().Name));
            return Decorate(select, queryBuilder);
        }


        protected abstract TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder);
    }
}
