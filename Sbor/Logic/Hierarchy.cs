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
        /// Возвращает всех потомков записи (исключая текущего и до листьев включительно)
        /// </summary>
        public static List<T> GetDescendants<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                  T anchor,
                                                  Func<T, TId> idSelector,
                                                  Func<T, TId?> idParentSelector
            ) where TId : struct
        {
            var a = hierarchicalCollection.Join(
                hierarchicalCollection.GetDescendantsIds(idSelector(anchor), idSelector, idParentSelector),
                idSelector,
                id => id,
                (o, i) => o);
            return null;
        }

        /// <summary>
        /// Возвращает Id всех потомков записи (исключая текущего и до листьев включительно)
        /// </summary>
        public static List<TId> GetDescendantsIds<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                      TId anchorID,
                                                      Func<T, TId> idSelector,
                                                      Func<T, TId?> idParentSelector
            ) where TId : struct
        {
            return hierarchicalCollection.GetDescendantIds<T, TId>(anchorID, idSelector, idParentSelector, new List<TId>());
        }

        /// <summary>
        /// Возвращает Id всех потомков записи (исключая текущего и до листьев включительно)
        /// При поиске исключает переход по циклическим ссылкам
        /// </summary>
        private static List<TId> GetDescendantIds<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                       TId anchorID,
                                                       Func<T, TId> idSelector,
                                                       Func<T, TId?> idParentSelector,
                                                       IEnumerable<TId> parentIds //необходим для исключения циклических ссылок
            ) where TId : struct
        {
            List<TId> childrenIds = new List<TId>();
            foreach (
                TId childId in
                    hierarchicalCollection.Where(w => idParentSelector(w).Equals(anchorID))
                                          .Select(idSelector)
                                          .Distinct())
            {
                if (!parentIds.Contains(childId)) //пропуск циклических ссылок
                {
                    childrenIds.Add(childId);
                    childrenIds.AddRange(hierarchicalCollection.GetDescendantIds<T, TId>(childId, idSelector, idParentSelector,
                                                                                   childrenIds));
                }

            }
            return childrenIds.Distinct().ToList();
        }

        //Возвращает идентификаторы дочерних записей для элемента А(непосредственно подчинённые, у которых родитель является элементом А) 
        public static IEnumerable<TId> GetChildrenIds<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                        TId anchorId,
                                                        Func<T, TId> idSelector,
                                                        Func<T, TId?> parentIdSelector) where TId : struct
        {
            return hierarchicalCollection.Where(w => parentIdSelector(w).Equals(anchorId)).Select(idSelector);
        }
    }
}
