using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Client.Filters;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common.Interfaces;
using Order = Platform.Utils.Common.Order;
using Platform.Client.Filters.Extensions;

namespace Platform.BusinessLogic.DataAccess.ClientFilters
{
    public class ClientFiltersForExtendedFieldsDecorator : AddWhere, IOrdered
    {
        private IEnumerable<ClientFilter> clientFilters;

        public ClientFiltersForExtendedFieldsDecorator(IEnumerable<ClientFilter> clientFilters)
            :base(null, LogicOperator.And, true)
        {
            this.clientFilters = clientFilters;
            _createColumn = false;
        }

        /*
         * ToDo{CORE-1014} Декоратор не должен быть толстым. 
         * Поэтому инструкцию (FilterConditions)ClientFilterConditionsFactory.Create(clientFilters.ForExtendedFields(queryBuilder.Entity));
         * следует вынести за пределы декоратора, а данный декоратор будет принимать только IFilterConditions filter (см. закомментированный конструктор ниже).
         * Для этого придется все декораторы регистрировать в DataManager'е, т.к. перед регистрацией данного декоратора нужно знать queryBuilder.Entity (см. строчку выше).
         * А queryBuilder.Entity становится известной только после создания билдера.
         * Кроме того после данной доработки мы сможем привести все sql-декораторы собственно к паттерну Декоратор.
         * Т.о. это позволит отвязать декоратор от билдера. Далеко не всем декораторам нужен билдер. Некоторым достаточно списка полей.
         * Т.о. все декораторы будут более отвечать требованию http://ru.wikipedia.org/wiki/Принцип_единственной_обязанности.
         * 
         * public ClientFiltersForExtendedFieldsDecorator(IFilterConditions filter)
            : base(filter, LogicOperator.And, true)
        {
        }*/

        protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
        {
            _builder = (queryBuilder as IDeleteQueryBuilder); //ToDo{CORE-1014} копипаст с методом базового класса
            _filter = (FilterConditions)ClientFilterConditionsFactory.Create(clientFilters.ForExtendedFields(queryBuilder.Entity));

            if (_filter == null)
                return source;

            replaceAliasesWithExpressions((source as SelectStatement));
            return base.DoDecorate(source, queryBuilder);
        }

        private void replaceAliasesWithExpressions(SelectStatement selectStatement)
        {
            Action<IFilterConditions> action = node =>
            {
                if (node.Type == LogicOperator.Simple
                    && !_builder.Entity.RealFields.Any(f =>
                        f.Name.Equals(node.Field, StringComparison.InvariantCultureIgnoreCase)
                        )
                    )
                {
                    node.Field = selectStatement.GetSelectColumn(node.Field).Render();
                }
            };

            _filter.ForEach(action);
        }

        #region Implementation of IOrdered

        public IEnumerable<Type> Before { get; private set; }
        public IEnumerable<Type> After
        {
            get
            {
                return new List<Type>
                    {
                        typeof(AddCaptions),
                        typeof(AddDescriptions),
                        typeof(AddJoinedFields)
                        //typeof(MultilinkAsString)
                    };
            }
        }
        public Order WantBe { get; private set; }

        #endregion
    }
}
