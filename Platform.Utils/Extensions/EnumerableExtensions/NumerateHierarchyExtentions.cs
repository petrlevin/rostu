using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Utils.Extensions
{
    #region Устарело, построение основвываясь на интерфейсе
    //public interface IHierachy
    //{
    //    int Id { get; set; }
    //    int? IdParent { get; set; }
    //    string HierarchyNumber { get; set; }
    //}
    #endregion

    public static class NumerateHierarchyExtentions
    {
        #region Устарело, построение основвываясь на интерфейсе
        ///// <summary>
        ///// Посроение нумерации иерархии, способен обрабатывать неуникальные наборы данных
        ///// </summary>
        ///// <param name="data">набор данных иерархии</param>
        ///// <param name="prefix">префикс</param>*/
        //public static void NumerateHierarchy(this IEnumerable<IHierachy> data, string prefix = "")
        //{
        //    data.NumerateHierarchy(d => d.Id, d => d.IdParent, (d, num) => d.HierarchyNumber = num, prefix);
        //                //Стартовые (якорные) элементы для постороения иерархии
        //                //List<int> rootData = data.Except( //исключаем всех, имеющих родителя в данном наборе
        //                //    data.Join(data, parent => parent.Id, child => child.IdParent, (parent, child) => child))
        //                //                                .Select(s => s.Id).ToList();

        //                //NumerateHierarchy(data, rootData, prefix);
        //}

        ///// <summary>
        ///// Посроение нумерации иерархии, способен обрабатывать неуникальные наборы данных
        ///// </summary>
        ///// <param name="data">Набор данных для обработки</param>
        ///// <param name="anchorIDs">Набор корневых элементов для начала построения иерархии</param>
        ///// <param name="prefix">Префикс нумерации</param>*/
        //public static void NumerateHierarchy(this IEnumerable<IHierachy> data, List<int?> anchorIDs, string prefix = "")
        //{
        //    data.NumerateHierarchy(d => d.Id, d => d.IdParent, (d, num) => d.HierarchyNumber = num, (List<int?>)anchorIDs,
        //                           prefix);
        //    //int i = 0;
        //    //foreach (int parentGroup in data.Join(anchorIDs, d => d.Id, a => a, (d, a) => d.Id).Distinct())  //для каждой группы с одинаковым Id
        //    //{
        //    //    i++;
        //    //    string hierarchyHumber = prefix + i.ToString() + ".";
        //    //    data.Where(w => w.Id == parentGroup).ToList().ForEach(f => f.HierarchyNumber = hierarchyHumber);
        //    //    List<int> child = data.Where(w => w.IdParent == parentGroup).Select(s => s.Id).ToList();
        //    //    if (child.Any())
        //    //    {
        //    //        NumerateHierarchy(data, child, hierarchyHumber);
        //    //    }
        //    //}
        //}

        ////только для иерархии с уникальным ИД
        //private static void NumerateHierarchyUi(this IEnumerable<IHierachy> hierarchicalCollection, List<int> anchorIDs, string prefix = "")
        //{
        //    int i = 0;
        //    foreach (IHierachy parent in hierarchicalCollection.Join(anchorIDs, d => d.Id, a => a, (d, a) => d))
        //    {
        //        i++;

        //        string hierarchyHumber = prefix + i.ToString() + ".";
        //        parent.HierarchyNumber = hierarchyHumber;
        //        hierarchicalCollection.Where(w => w.Id == parent.Id).ToList().ForEach(f => f.HierarchyNumber = hierarchyHumber);
        //        List<int> child = hierarchicalCollection.Where(w => w.IdParent == parent.Id).Select(s => s.Id).ToList();
        //        if (child.Any())
        //        {
        //            NumerateHierarchyUi(hierarchicalCollection, child, hierarchyHumber);
        //        }
        //    }
        //}
        #endregion


        /// <summary>
        /// Нумерация иерархии (для коллекций с идентификатором НЕ ДОПУСКАЮЩИМ нулевые значения)
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TId">тип идентификатора</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="hierarchicalCollection">иерахическая коллекция элементов типа T</param>
        /// <param name="idSelector">селектор идентификатора</param>
        /// <param name="parentIdSelector">селектор поля указывающего на родителя</param>
        /// <param name="orderKeySelector">селектор для указания сортировки</param>
        /// <param name="numberUpdater">действие, для установки номера</param>
        /// <param name="prefix">префикс номера</param>
        /// <param name="delimiter">разделитель, по умолчанию "."</param>
        /// <param name="orderDescending">сортировать по убыванию, по умолчанию = ложь</param>
        public static void NumerateHierarchy<T, TId, TOrderKey>(this IEnumerable<T> hierarchicalCollection,
                                                                Func<T, TId> idSelector,  //c ненулевым Id
                                                                Func<T, TId?> parentIdSelector,
                                                                Func<T, TOrderKey> orderKeySelector,
                                                                Action<T, string> numberUpdater,
                                                                string prefix = "",
                                                                string delimiter = ".",
                                                                bool orderDescending = false)
            where TId : struct
            where TOrderKey : IComparable
        {
            hierarchicalCollection.NumerateHierarchy(s => (TId?)idSelector(s),
                            parentIdSelector, orderKeySelector, numberUpdater, prefix, delimiter, orderDescending);
        }


        /// <summary>
        /// Нумерация иерархии (для коллекций с идентификатором ДОПУСКАЮЩИМ нулевые значения)
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TId">тип идентификатора</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="hierarchicalCollection">иерахическая коллекция элементов типа T</param>
        /// <param name="idSelector">селектор идентификатора</param>
        /// <param name="parentIdSelector">селектор поля указывающего на родителя</param>
        /// <param name="orderKeySelector">селектор для указания сортировки</param>
        /// <param name="numberUpdater">действие, для установки номера</param>
        /// <param name="prefix">префикс номера</param>
        /// <param name="delimiter">разделитель, по умолчанию "."</param>
        /// <param name="orderDescending">сортировать по убыванию, по умолчанию = ложь</param>
        public static void NumerateHierarchy<T, TId, TOrderKey>(this IEnumerable<T> hierarchicalCollection,
                                                                Func<T, TId?> idSelector,// c Id допускающим null
                                                                Func<T, TId?> parentIdSelector,
                                                                Func<T, TOrderKey> orderKeySelector,
                                                                Action<T, string> numberUpdater,
                                                                string prefix = "",
                                                                string delimiter = ".",
                                                                bool orderDescending = false)
                                                                    where TId : struct
                                                                    where TOrderKey : IComparable
        {
            //Стартовые (якорные) элементы для постороения иерархии
            List<TId?> rootData =
                hierarchicalCollection.Select(idSelector)
                                      .Except( //исключаем всех, имеющих родителя в данном наборе
                                          hierarchicalCollection.Join(hierarchicalCollection,
                                                                      parent => idSelector(parent),
                                                                      child => parentIdSelector(child),
                                                                      (parent, child) => idSelector(child))).ToList();
            hierarchicalCollection.NumerateHierarchy(idSelector, parentIdSelector, orderKeySelector, numberUpdater, rootData, prefix, delimiter, orderDescending);
        }

        /// <summary>
        /// Нумерация ЧАСТИ иерархии (для коллекций с идентификатором НЕ ДОПУСКАЮЩИМ нулевые значения). Необходимо указать якорные элементы
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TId">тип идентификатора</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="hierarchicalCollection">иерахическая коллекция элементов типа T</param>
        /// <param name="idSelector">селектор идентификатора</param>
        /// <param name="parentIdSelector">селектор поля указывающего на родителя</param>
        /// <param name="orderKeySelector">селектор для указания сортировки</param>
        /// <param name="numberUpdater">действие, для установки номера</param>
        /// <param name="anchorIDs">якорные/опорные элементы, начиная от которых вглубь рекурсивно идет нумерация</param>
        /// <param name="prefix">префикс номера</param>
        /// <param name="delimiter">разделитель, по умолчанию "."</param>
        /// <param name="orderDescending">сортировать по убыванию, по умолчанию = ложь</param>
        public static void NumerateHierarchy<T, TId, TOrderKey>(this IEnumerable<T> hierarchicalCollection,
                                                                Func<T, TId> idSelector,  //c ненулевым Id
                                                                Func<T, TId?> parentIdSelector,
                                                                Func<T, TOrderKey> orderKeySelector,
                                                                Action<T, string> numberUpdater,
                                                                List<TId?> anchorIDs,
                                                                string prefix = "",
                                                                string delimiter = ".",
                                                                bool orderDescending = false)
            where TId : struct
            where TOrderKey : IComparable
        {
            NumerateHierarchy(hierarchicalCollection, s => (TId?)idSelector(s), parentIdSelector, orderKeySelector, numberUpdater, anchorIDs, prefix, delimiter, orderDescending);
        }

        /// <summary>
        /// Нумерация ЧАСТИ иерархии (для коллекций с идентификатором ДОПУСКАЮЩИМ нулевые значения). Необходимо указать якорные элементы
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TId">тип идентификатора</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="hierarchicalCollection">иерахическая коллекция элементов типа T</param>
        /// <param name="idSelector">селектор идентификатора</param>
        /// <param name="parentIdSelector">селектор поля указывающего на родителя</param>
        /// <param name="orderKeySelector">селектор для указания сортировки</param>
        /// <param name="numberUpdater">действие, для установки номера</param>
        /// <param name="anchorIDs">якорные/опорные элементы, начиная от которых вглубь рекурсивно идет нумерация.</param>
        /// <param name="prefix">префикс номера</param>
        /// <param name="delimiter">разделитель, по умолчанию "."</param>
        /// <param name="orderDescending">сортировать по убыванию, по умолчанию = ложь</param>
        /// <remarks>Выполняет подготовительные операции над якорными элементами, затем вызывает рекурсию нумерации.</remarks>
        public static void NumerateHierarchy<T, TId, TOrderKey>(this IEnumerable<T> hierarchicalCollection,
                                                                Func<T, TId?> idSelector, // c Id допускающим null
                                                                Func<T, TId?> parentIdSelector,
                                                                Func<T, TOrderKey> orderKeySelector,
                                                                Action<T, string> numberUpdater,
                                                                List<TId?> anchorIDs,
                                                                string prefix = "",
                                                                string delimiter = ".",
                                                                bool orderDescending = false)
                                                                    where TId : struct
                                                                    where TOrderKey : IComparable
        {
            List<TId?> anchorIdsОrdered;
            if (!orderDescending)
                anchorIdsОrdered = hierarchicalCollection.Join(anchorIDs, idSelector, a => a, (c, a) => c).OrderBy(orderKeySelector).Select(idSelector).Distinct().ToList();
            else
                anchorIdsОrdered = hierarchicalCollection.Join(anchorIDs, idSelector, a => a, (c, a) => c).OrderByDescending(orderKeySelector).Select(idSelector).Distinct().ToList();
            
            hierarchicalCollection.NumerateHierarchyRecursion(idSelector, parentIdSelector, orderKeySelector, numberUpdater, anchorIdsОrdered, prefix, delimiter, orderDescending);
        }


        /// <summary>
        /// Нумерация ЧАСТИ иерархии (для коллекций с идентификатором ДОПУСКАЮЩИМ нулевые значения). Необходимо указать якорные элементы
        /// </summary>
        /// <typeparam name="T">элемент коллекции</typeparam>
        /// <typeparam name="TId">тип идентификатора</typeparam>
        /// <typeparam name="TOrderKey">тип ключа для сортировки перед нумерацией</typeparam>
        /// <param name="hierarchicalCollection">иерахическая коллекция элементов типа T</param>
        /// <param name="idSelector">селектор идентификатора</param>
        /// <param name="parentIdSelector">селектор поля указывающего на родителя</param>
        /// <param name="orderKeySelector">селектор для указания сортировки</param>
        /// <param name="numberUpdater">действие, для установки номера</param>
        /// <param name="anchorIDs">якорные/опорные элементы, начиная от которых вглубь рекурсивно идет нумерация. В целях оптимизации быстродействия подразумевается, что они уже проверены на уникальность, проверены на присутствие в коллекции и отсортированы.</param>
        /// <param name="prefix">префикс номера</param>
        /// <param name="delimiter">разделитель, по умолчанию "."</param>
        /// <param name="orderDescending">сортировать по убыванию, по умолчанию = ложь</param>
        /// <remarks>Выполняется после подготовительных операций над якорными элементами</remarks>
        private static void NumerateHierarchyRecursion<T, TId, TOrderKey>(this IEnumerable<T> hierarchicalCollection,
                                                                Func<T, TId?> idSelector, // c Id допускающим null
                                                                Func<T, TId?> parentIdSelector,
                                                                Func<T, TOrderKey> orderKeySelector,
                                                                Action<T, string> numberUpdater,
                                                                List<TId?> anchorIDs,
                                                                string prefix = "",
                                                                string delimiter = ".",
                                                                bool orderDescending = false)
                                                                    where TId : struct
                                                                    where TOrderKey : IComparable
        {
            int i = 0;
            foreach (TId? parentGroup in anchorIDs)
            {
                i++;
                string hierarchyNumber = prefix + i.ToString() + delimiter;
                hierarchicalCollection.Where(w => idSelector(w).Equals(parentGroup)).ToList().ForEach(f => numberUpdater(f, hierarchyNumber));

                List<TId?> child;
                if (!orderDescending)
                    child = hierarchicalCollection.Where(w => parentIdSelector(w).Equals(parentGroup)).OrderBy(orderKeySelector).Select(idSelector).Distinct().ToList();
                else
                    child = hierarchicalCollection.Where(w => parentIdSelector(w).Equals(parentGroup)).OrderByDescending(orderKeySelector).Select(idSelector).Distinct().ToList();

                if (child.Any())
                {
                    hierarchicalCollection.NumerateHierarchy(idSelector, parentIdSelector, orderKeySelector, numberUpdater, child, hierarchyNumber, delimiter, orderDescending);
                }
            }
        }


        //Возвращает идентификаторы дочерних записей для элемента А(непосредственно подчинённые, у которых родитель является элементом А) 
        public static IEnumerable<TId> Children<T, TId>(this IEnumerable<T> hierarchicalCollection,
                                                        Func<T, TId> idSelector,
                                                        Func<T, TId?> parentIdSelector,
                                                        TId anchorId) where TId : struct
        {
            return hierarchicalCollection.Where(w => parentIdSelector(w).Equals(anchorId)).Select(idSelector);
        }

        //Возвращает дочерние записи для элемента (непосредственно подчинённые, у которых родитель является элементом А)

        //Возвращает ближайшего потомка, соответствующего условию, и расстояние

        //Возвращает идентификаторы потомков для элемента и расстояние




        //Возвращает потомков для элемента и расстояние

        //Возвращает потомков элемента, но не дальше предка-условия






        //Возвращает идентификаторы дочерних записей для списка элементов

        //Возвращает дочерние записи для списка элементов

        //Возвращает идентификаторы потомков для списка элементов и расстояние

        //Возвращает идентификаторы потомков для списка элементов и расстояние


        //Возвращает потомков для списка элементов и расстояние





        //Возвращает идентификатор родителя элемента

        //Возвращает родителя элемента


        //Возвращает ближайшего предка, соответствующего условию, и расстояние

        //Возвращает идентификаторы предков элемента и расстояние

        //Возвращает предков элемента и расстояние
        //predecessors

        //Возвращает предков элемента, но не дальше предка-условия
    }
}
