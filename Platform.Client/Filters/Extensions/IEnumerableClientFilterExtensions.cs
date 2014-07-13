using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.Client.Filters.Extensions
{
    public static class IEnumerableClientFilterExtensions
    {
        private static Func<ClientFilter, IEntity, bool> real = (clientFilter, entity) =>
            entity.RealFields.Any(realField =>
                realField.Name.Equals(clientFilter.Field, StringComparison.InvariantCultureIgnoreCase));

        /// <summary>
        /// Отбирает только фильтры полей, которым соответствует колонка в таблице БД (RealField)
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<ClientFilter> ForRealFields(this IEnumerable<ClientFilter> filters, IEntity entity)
        {
            return filters.Where(cf => real(cf, entity));
        }

        /// <summary>
        /// Отбирает только фильтры расширенных полей, т.е. которым не соответствует колонка в таблице БД.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<ClientFilter> ForExtendedFields(this IEnumerable<ClientFilter> filters, IEntity entity)
        {
            return filters.Where(cf => !real(cf, entity));
        }
    }
}
