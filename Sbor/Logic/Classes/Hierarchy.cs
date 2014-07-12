using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Logic.Hierarchy
{
    public static class Hierarchy
    {
        /// <summary>
        /// Возвращает Id и расстояние до ближайшего предка, удовлетворяющего условию findCondition
        /// При поиске исключает переход по циклическим ссылкам.
        /// </summary>
        /// <typeparam name="T">тип элемента иерархии</typeparam>
        /// <typeparam name="TId">тип идентификатора элемента иерархии</typeparam>
        /// <param name="hierarchicalCollection">коллеккция элементов иерархии</param>
        /// <param name="anchorId">стартовый элемент для поиска предков</param>
        /// <param name="idSelector">селектор для поля идентификатора</param>
        /// <param name="idParentSelector">селектор для поля идентификатора родителя</param>
        /// <param name="findCondition"></param>
        /// <param name="beforeNearest"></param>
        /// <returns>Возвращает идентификатор передка(Key) и расстояние до него(Value) от стартового элемента (дистанция до элемента соседнего со стартовым равна единице)</returns>
        public static KeyValuePair<TId?, int?> NearestParentId<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                                   TId anchorId,
                                                                   Func<T, TId> idSelector,
                                                                   Func<T, TId?> idParentSelector,
                                                                   Func<T, bool> findCondition,
                                                                   bool beforeNearest = false) where TId : struct
        {
            KeyValuePair<TId?, int?> result;
            Dictionary<TId, int> parents = GetParentsIds(hierarchicalCollection, anchorId, idSelector, idParentSelector, findCondition, !beforeNearest);
            if (parents.Any())
            {
                var last = parents.Last();
                result = new KeyValuePair<TId?, int?>(last.Key,last.Value);
            }
            else
            {
                result = new KeyValuePair<TId?, int?>(null, null);
            }
            return result;
        }


        /// <summary>
        /// Возвращает упорядоченный словарь Id всех предков и расстояние до них от стартового элемента, от текущего(исключая) и до корня(по умолчанию включительно) или до элемента,
        /// удовлетворяющего stopCondition, что встретится раньше.
        /// При поиске исключает переход по циклическим ссылкам.
        /// </summary>
        /// <typeparam name="T">тип элемента иерархии</typeparam>
        /// <typeparam name="TId">тип идентификатора элемента иерархии</typeparam>
        /// <param name="hierarchicalCollection">коллеккция элементов иерархии</param>
        /// <param name="anchorId">стартовый элемент для поиска предков</param>
        /// <param name="idSelector">селектор для поля идентификатора</param>
        /// <param name="idParentSelector">селектор для поля идентификатора родителя</param>
        /// <param name="stopCondition">условие для элемента иерархии, на котором отановить восхождение</param>
        /// <param name="includeLastElement">флаг включения в результат элемента, на котором прервалось восхождение</param>
        /// <returns>Возвращает идентификаторы передков(Key) и расстояние(Value) до них от стартового элемента, начиная с единицы</returns>
        public static Dictionary<TId, int> GetParentsIds<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                       TId anchorId,
                                                       Func<T, TId> idSelector,
                                                       Func<T, TId?> idParentSelector,
                                                       Func<T, bool> stopCondition,
                                                       bool includeLastElement = true) where TId : struct
        {
            var result = new Dictionary<TId, int>();


            //получаем id родителя для якоря
            var currentId = hierarchicalCollection.Where(w => idSelector(w).Equals(anchorId) && !idParentSelector(w).Equals(anchorId)).Select(s => idParentSelector(s)).FirstOrDefault();

            int distance = 0;

            //повтор цикла пока есть id для поиска родителя
            while (currentId.HasValue)
            {
                //приращение шага дистанции
                distance++;

                //получаем id родителя, учитывая, что текущий не соответствует стоп-условию поиска предков
                var parentId = hierarchicalCollection.Where(w => idSelector(w).Equals(currentId) && !stopCondition(w)).Select(s => idParentSelector(s)).FirstOrDefault();

                //проверяем, что не вызвана остановка
                if (parentId.HasValue)
                {
                    //добавляем в найденное id текущего и расстояние
                    result.Add(currentId.Value, distance);
                }
                else
                {
                    //в соответствии с флагом включаем в набор предка, вызвавшего остановку, если его ещё нет в списке
                    if (includeLastElement) result.Add(currentId.Value, distance);
                    break;
                }
                
                //проверяем, что id родителя не встретился в уже найденном, защита от зацикливания
                if (parentId.Equals(anchorId) || result.ContainsKey(parentId.Value)) break;
                   
                //присваиваем текущему id поиска id родителя
                currentId = parentId;
            }

            return result;
        }

       /// <summary>
        /// Возвращает Id всех потомков записи (исключая текущего и до листьев включительно)
        /// </summary>
        public static List<TId> GetDescendantsIds<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                      TId anchorID,
                                                      Func<T, TId> idSelector,
                                                      Func<T, TId?> idParentSelector) where TId : struct
        {
            return hierarchicalCollection.GetDescendantsIds<T, TId>(anchorID, idSelector, idParentSelector, new List<TId>());
        }

        /// <summary>
        /// Возвращает Id всех потомков записи (исключая текущего и до листьев включительно)
        /// При поиске исключает переход по циклическим ссылкам
        /// </summary>
        private static List<TId> GetDescendantsIds<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                       TId anchorID,
                                                       Func<T, TId> idSelector,
                                                       Func<T, TId?> idParentSelector,
                                                       IEnumerable<TId> parentIds /*необходим для исключения циклических ссылок*/ ) where TId : struct
        {
            List<TId> descendantsIds = new List<TId>();
            List<TId> childrenIds = hierarchicalCollection.Where(w => idParentSelector(w).Equals(anchorID))
                                                          .Select(idSelector)
                                                          .Distinct().ToList();
            foreach (TId childId in childrenIds)
            {
                if (!parentIds.Contains(childId)) //пропуск циклических ссылок
                {
                    descendantsIds.Add(childId);
                    descendantsIds.AddRange(hierarchicalCollection.GetDescendantsIds(childId, idSelector, idParentSelector, descendantsIds));
                }
            }
            return descendantsIds.Distinct().ToList();
        }

        /// <summary>
        /// Возвращает идентификаторы дочерних записей для элемента А(непосредственно подчинённые, у которых родитель является элементом А) 
        /// </summary>
        /// <param name="hierarchicalCollection"></param>
        /// <param name="anchorId"></param>
        /// <param name="idSelector"></param>
        /// <param name="parentIdSelector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TId> GetChildrenIds<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                        TId anchorId,
                                                        Func<T, TId> idSelector,
                                                        Func<T, TId?> parentIdSelector) where TId : struct
        {
            return hierarchicalCollection.Where(w => parentIdSelector(w).Equals(anchorId)).Select(idSelector);
        }
    }
}
