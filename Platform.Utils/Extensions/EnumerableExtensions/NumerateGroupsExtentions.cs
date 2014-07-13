using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;

namespace Platform.Utils.Extensions
{
    public static class NumerateGroupsExtentions
    {
        /// <summary>
        /// Нумерация внутри группы A уникальных групп B части коллекции c ключем для сортировки
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TBlockId">тип идентификатора группы A</typeparam>
        /// <typeparam name="TGroupId">тип идентификатора группы B</typeparam>
        /// <typeparam name="TGroupId">тип идентификатора группы B</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="collection">коллекция элементов типа T</param>
        /// <param name="numerateInSelector">идентификатор для группы A, поле по которому происходит объединение элементов в группы A</param>
        /// <param name="numerateByGroupSelector">идентификатор для группы B, поле по которому происходит объединение элементов в группы B</param>
        /// <param name="setter">Действие, для установки номера</param>
        /// <param name="orderKeySelector">ключ для сортировки перед нумерацией, должен быть частью идентификатора группы B</param>
        /// <param name="whereSelector">выражение отбора части коллекции для обработки</param>
        /// <param name="orderDescending">направление сортировки по ключу orderKeySelector</param>
        public static void NumerateInternalGroupsInWhere<T, TBlockId, TGroupId, TOrderKey>(
            this IEnumerable<T> collection,
            Func<T, TBlockId> numerateInSelector,
            Func<T, TGroupId> numerateByGroupSelector,
            Func<TGroupId, TOrderKey> orderKeySelector,
            Func<T, bool> whereSelector,
            Action<T, int> setter,
            bool orderDescending = false)
                where TBlockId : IEquatable<TBlockId> //допустимо снять это ограничение, сравнение идет толко с тем же селекторорм
                where TOrderKey : IComparable
        {
            int i;

            foreach (TBlockId inGroup in collection.Where(whereSelector)
                                              .Select(numerateInSelector).Distinct().ToList())
            {
                List<TGroupId> byGroupList;
                var tmp = collection
                            .Where(w => whereSelector(w)
                                    && (inGroup == null ? numerateInSelector(w) == null
                                                        : numerateInSelector(w) == null ? false
                                                                                        : numerateInSelector(w).Equals(inGroup)))
                    //.Select(s => new { groupKey = numerateByGroupSelector(s), orderKey = orderKeySelector(s) }) //замененно в связи с тем, что orderKeySelector должен быть частью идентификатора группы B
                            .Select(numerateByGroupSelector)
                            .Distinct();
                if (!orderDescending)
                    //byGroupList = tmp.OrderBy(o => o.orderKey).Select(s => s.groupKey).ToList(); //замененно в связи с тем, что orderKeySelector должен быть частью идентификатора группы B
                    byGroupList = tmp.OrderBy(orderKeySelector).ToList();
                else
                    //byGroupList = tmp.OrderByDescending(o => o.orderKey).Select(s => s.groupKey).ToList(); //замененно в связи с тем, что orderKeySelector должен быть частью идентификатора группы B
                    byGroupList = tmp.OrderByDescending(orderKeySelector).ToList();


                i = 0;
                foreach (TGroupId byGroup in byGroupList)
                {
                    i++;
                    collection.Where(w => whereSelector(w)
                                    && (inGroup == null ? numerateInSelector(w) == null
                                                        : numerateInSelector(w) == null ? false
                                                                                        : numerateInSelector(w).Equals(inGroup))
                                    && (byGroup == null ? numerateByGroupSelector(w) == null
                                                        : numerateByGroupSelector(w) == null ? false
                                                                                             : numerateByGroupSelector(w).Equals(byGroup))
                                    )
                        .ForEach(f => setter(f, i));
                }
            }
        }

        /// <summary>
        /// Нумерация внутри группы A уникальных групп B (сортировка по B)
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TBlockId">тип идентификатора группы A</typeparam>
        /// <typeparam name="TGroupId">тип идентификатора группы B</typeparam>
        /// <param name="collection">коллекция элементов типа T</param>
        /// <param name="numerateInSelector">идентификатор для группы A, поле по которому происходит объединение элементов в группы A</param>
        /// <param name="numerateByGroupSelector">идентификатор для группы B, поле по которому происходит объединение элементов в группы B</param>
        /// <param name="setter">Действие, для установки номера.</param>
        public static void NumerateInternalGroups<T, TBlockId, TGroupId>(
            this List<T> collection,
            Func<T, TBlockId> numerateInSelector,
            Func<T, TGroupId> numerateByGroupSelector,
            Action<T, int> setter)
                where TBlockId : IEquatable<TBlockId>
                where TGroupId : IComparable
        {
            NumerateInternalGroupsInWhere(collection, numerateInSelector, numerateByGroupSelector, o => o, w => true, setter);
        }

        /// <summary>
        /// Нумерация уникальных групп части коллекции c ключем для сортировки
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TGroupId">тип идентификатора группы</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="collection">коллекция элементов типа T</param>
        /// <param name="numerateByGroupSelector">идентификатор для группы, поле по которому происходит объединение элементов в группы</param>
        /// <param name="setter">Действие, для установки номера.</param>
        /// <param name="orderKeySelector">ключ для сортировки перед нумерацией, должен быть частью идентификатора для группы</param>
        /// <param name="whereSelector">выражение отбора части коллекции для обработки</param>
        /// <param name="orderDescending">направление сортировки по ключу orderKeySelector</param>
        public static void NumerateGroups<T, TGroupId, TOrderKey>(
            this IEnumerable<T> collection,
            Func<T, TGroupId> numerateByGroupSelector,
            //Func<T, TOrderKey> orderKeySelector, //замененно в связи с тем, что orderKeySelector должен быть частью идентификатора группы B
            Func<TGroupId, TOrderKey> orderKeySelector,
            Func<T, bool> whereSelector,
            Action<T, int> setter,
            bool orderDescending = false)
                where TOrderKey : IComparable
        {

            List<TGroupId> groupIdList;
            var tmp = collection
                        .Where(whereSelector)
                //.Select(s => new { groupKey = numerateByGroupSelector(s), orderKey = orderKeySelector(s) }) //замененно в связи с тем, что orderKeySelector должен быть частью идентификатора группы
                        .Select(numerateByGroupSelector)
                        .Distinct();
            if (!orderDescending)
                //groupIdList = tmp.OrderBy(o => o.orderKey).Select(s => s.groupKey).ToList(); //замененно в связи с тем, что orderKeySelector должен быть частью идентификатора группы
                groupIdList = tmp.OrderBy(orderKeySelector).ToList();
            else
                //groupIdList = tmp.OrderByDescending(o => o.orderKey).Select(s => s.groupKey).ToList();  //замененно в связи с тем, что orderKeySelector должен быть частью идентификатора группы
                groupIdList = tmp.OrderByDescending(orderKeySelector).ToList();

            int i = 0;
            foreach (TGroupId gId in groupIdList)
            {
                i++;
                collection.Where(w => whereSelector(w)
                                    && (gId == null ? numerateByGroupSelector(w) == null
                                                    : numerateByGroupSelector(w) == null ? false
                                                                                         : numerateByGroupSelector(w).Equals(gId)))
                              .ForEach(f => setter(f, i));
            }
        }

        /// <summary>
        /// Нумерация уникальных групп c ключем для сортировки
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TGroupId">тип идентификатора группы</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="collection">коллекция элементов типа T</param>
        /// <param name="numerateByGroupSelector">идентификатор для группы, поле по которому происходит объединение элементов в группы</param>
        /// <param name="orderKeySelector">ключ для сортировки перед нумерацией, должен быть частью идентификатора для группы</param>
        /// <param name="setter">Действие, для установки номера.</param>
        /// <param name="orderDescending">направление сортировки по ключу orderKeySelector</param>
        public static void NumerateGroups<T, TGroupId, TOrderKey>(
                this IEnumerable<T> collection,
                Func<T, TGroupId> numerateByGroupSelector,
                Func<TGroupId, TOrderKey> orderKeySelector,
                Action<T, int> setter,
                bool orderDescending = false)
                    where TOrderKey : IComparable
        {
            NumerateGroups(collection, numerateByGroupSelector, orderKeySelector, w => true, setter, orderDescending);
        }


        /// <summary>
        /// Нумерация уникальных групп
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TGroupId">тип идентификатора группы</typeparam>
        /// <param name="collection">коллекция элементов типа T</param>
        /// <param name="numerateByGroupSelector">идентификатор для группы, поле по которому происходит объединение элементов в группы</param>
        /// <param name="setter">Действие, для установки номера.</param>
        public static void NumerateGroups<T, TGroupId>(this IEnumerable<T> collection, Func<T, TGroupId> numerateByGroupSelector, Action<T, int> setter) where TGroupId : IComparable
        {
            NumerateGroups(collection, numerateByGroupSelector, o => o, w => true, setter);
        }

    }
}
