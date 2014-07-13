using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Utils.Extensions
{
    public static class Range
    {
        /// <summary>
        /// Дополняет коллекцию элементами, если значения указанного свойства элементов не полностью покрывают заданный диапазон
        /// Принимает базовый элемент для вставки и функцию для егокорректировки изменения непосредственно перед вставкой
        /// Типичное применение: дополнение коллекций данных недостающими годами, чтобы при выводе отчета диапазон оторажаемых годов был непрерывным
        /// </summary>
        /// <typeparam name="T">тип элемента</typeparam>
        /// <param name="collection">коллекция</param>
        /// <param name="rangeStart">начало диапазона</param>
        /// <param name="rangeEnd">конец диапазона</param>
        /// <param name="fieldInRangeSelector">свойство элемента для проверки на соответствие диапазону</param>
        /// <param name="changeProp">функция, для внесения изменений в элемент objToAdd, перед его добавлением в коллекцию, в зависимости от найденного пропуска i в диапазаоне rangeStart-rangeEnd</param>
        /// <param name="objToAdd">базовый объект для вставки, будет добавлен с изменённым значением указанным согласно changeProp </param>
        public static void AddMissingInRange<T>(this List<T> collection, int rangeStart, int rangeEnd, Func<T, int?> fieldInRangeSelector, Func<T, int, T> changeProp, T objToAdd)
        {
            List<int?> existing = collection.Select(fieldInRangeSelector).Distinct().ToList();
            for (int i = rangeStart; i <= rangeEnd; i++)
            {
                if (!existing.Contains(i))
                {
                    collection.Add(changeProp(objToAdd, i));
                }
            }
        }

        /// <summary>
        /// Дополняет коллекцию элементами, если значения указанного свойства элементов не полностью покрывают заданный диапазон
        /// В качестве базового элемента берется первый элемент коллекции
        /// Принимает функцию для изменения базового элемента неосредственно перед вставкой
        /// Типичное применение: дополнение коллекций данных недостающими годами, чтобы при выводе отчета диапазон оторажаемых годов был непрерывным
        /// </summary>
        /// <typeparam name="T">тип элемента</typeparam>
        /// <param name="collection">коллекция</param>
        /// <param name="rangeStart">начало диапазона</param>
        /// <param name="rangeEnd">конец диапазона</param>
        /// <param name="fieldInRangeSelector">свойство элемента для проверки на соответствие диапазону</param>
        /// <param name="changeProp">функция, для внесения изменений в произвольный элемент коллекции, перед его добавлением в коллекцию, в зависимости от найденного пропуска i в диапазаоне rangeStart-rangeEnd</param>
        public static void AddMissingInRange<T>(this List<T> collection, int rangeStart, int rangeEnd, Func<T, int?> fieldInRangeSelector, Func<T, int, T> changeProp)
        {
            if (collection.Count != 0)
                AddMissingInRange(collection, rangeStart, rangeEnd, fieldInRangeSelector, changeProp, collection.First());
        }
    }
}
