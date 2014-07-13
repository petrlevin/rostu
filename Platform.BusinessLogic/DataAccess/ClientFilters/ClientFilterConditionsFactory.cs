using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Platform.Client.Filters;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;

namespace Platform.BusinessLogic.DataAccess.ClientFilters
{
    /// <summary>
    /// По <see cref="ClientFilter">клиентским фильтрам</see>
    /// получает объект <see cref="FilterConditions"/>.
    /// </summary>
    /// <seealso cref="GridParams.Filters"/>
    public class ClientFilterConditionsFactory
    {
        public static IFilterConditions Create(IEnumerable<ClientFilter> clientFilters, LogicOperator howToJoin = LogicOperator.And)
        {
            return Create(null, clientFilters, howToJoin);
        }

        public static IFilterConditions Create(IFilterConditions source, IEnumerable<ClientFilter> clientFilters, LogicOperator howToJoin = LogicOperator.And)
        {
            var instance = new ClientFilterConditionsFactory()
                {
                    source = source,
                    howToJoin = howToJoin,
                    clientFilters = clientFilters
                };
            return instance.get();
        }

        private IFilterConditions source { get; set; }
        private LogicOperator howToJoin { get; set; }
        private IEnumerable<ClientFilter> clientFilters { get; set; }

        private ClientFilterConditionsFactory()
        {
        }

        private IFilterConditions get()
        {
            if (!checkSucess())
                return source;
            if (source == null)
                return createList(clientFilters);

            return new FilterConditions()
                {
                    Type = howToJoin,
                    Operands =
                    {
                        source,
                        createList(clientFilters)
                    }
                };
        }

        private FilterConditions createList(IEnumerable<ClientFilter> clientFilters)
        {
            return new FilterConditions
                {
                    Type = LogicOperator.And,
                    Operands = clientFilters.Select(createItem).ToList()
                };
        }

        private IFilterConditions createItem(ClientFilter filter)
        {
            FilterConditions result = new FilterConditions
                {
                    Type = LogicOperator.And,
                    Operands = new List<IFilterConditions>()
                };

            foreach (FilterData filterData in filter.Data)
            {
                object value = filterData.Value;
                if (value is JArray)
                    value = ((JArray) value).ToObject<List<object>>();

                result.Operands.Add(new FilterConditions()
                {
                    Type = LogicOperator.Simple,
                    Field = filter.Field,
                    Operator = filterData.Comparison,
                    Value = value
                });
            }
            return result;
        }

        private bool checkSucess()
        {
            return clientFilters != null && clientFilters.Any();
        }
    }
}
