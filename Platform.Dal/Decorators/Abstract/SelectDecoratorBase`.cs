using System;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces.QueryBuilders;

namespace Platform.Dal.Decorators.Abstract
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    public abstract class SelectDecoratorBase<TImplementation, TQueryBuilder> : SelectDecoratorBase where TImplementation : ImplementationBase<SelectStatement, TQueryBuilder>, new() where TQueryBuilder : class, IQueryBuilder
    {
        protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
        {
            TQueryBuilder concreteQueryBuilder = queryBuilder as TQueryBuilder;
            if (concreteQueryBuilder== null)
                throw new ArgumentException("queryBuilder");
            var implementation = new TImplementation();
            implementation.QueryBuilder = concreteQueryBuilder;
            implementation.Source = source;
            InitImplementor(implementation);

            return implementation.Decorate();

        }

        protected abstract void InitImplementor(TImplementation implementor);
    }

    public abstract class SelectDecoratorBase<TImplementation> : SelectDecoratorBase<TImplementation, ISelectQueryBuilder> where TImplementation : ImplementationBase<SelectStatement, ISelectQueryBuilder>, new()
    {
        
    }
}

