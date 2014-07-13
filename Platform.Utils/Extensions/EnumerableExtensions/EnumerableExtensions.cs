using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Utils.Extensions
{
    static public class EnumerableExtensions
    {
        public static string ToString<T>(this IEnumerable<T> input, string concatinator, bool firstInline = true)
        {
            return ToString(input, concatinator, (e) => (e != null) ? e.ToString() : "", firstInline);
        }


        public static string ToHtmlList<T>(this IEnumerable<T> input)
        {
            return ToHtmlList(input,e=> (e != null) ? e.ToString() : "");
        }

        public static string ToHtmlList<T>(this IEnumerable<T> input, Func<T, string> toString)
        {
            if (input == null)
                return null;

            var outputString = new StringBuilder();
            outputString.Append("<ul>");
            foreach (var e in input)
            {
                outputString.Append("<li>").Append(toString(e)).Append("</li>");
            }
            outputString.Append("</ul>");
            return outputString.ToString();
            
        }

        public static string ToString<T>(this IEnumerable<T> input, Func<T, string> toString, bool firstInline = true)
        {
            return ToString(input, String.Empty, toString, true);
        }

        public static string ToString<T>(this IEnumerable<T> input, string concatinator, Func<T, string> toString, bool firstInline = true)
        {
            if (input == null)
                return null;

            var outputString = new StringBuilder();

            foreach (var e in input)
                if ((outputString.Length == 0) && firstInline)
                    outputString.Append(toString(e));
                else
                    outputString.Append(concatinator).Append(toString(e));

            return outputString.ToString();
        }

        public static string ToString<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> input,
                                                      string commonConcatinator,
            bool firstElementInGroupInline = false)
        {
            return input.ToString(commonConcatinator, (e) => e.ToString(), firstElementInGroupInline: firstElementInGroupInline);
        }


        public static string ToString<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> input,
                                                      string groupConcatinator, string elementConcatinator, bool firstElementInGroupInline = false)
            
        {
            return input.ToString(groupConcatinator, elementConcatinator, (e) => e.ToString(), firstElementInGroupInline: firstElementInGroupInline);
        }




        public static string ToString<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> input,
                                                      string commonConcatinator, Func<TElement, string> elementToString, bool firstElementInGroupInline = false)
        {
            return ToString(input, commonConcatinator, commonConcatinator, elementToString, firstElementInGroupInline);
        }


        public static string ToString<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> input,
                                                      string groupConcatinator, string elementConcatinator,

                                                      Func<TElement, string> elementToString, bool firstElementInGroupInline=false)
        {
            return ToString(input, groupConcatinator, elementConcatinator, gk => gk.ToString(), elementToString, firstElementInGroupInline);
        }

        public static string ToString<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> input, string groupConcatinator, string elementConcatinator, Func<TKey, string> groupKeyToString, Func<TElement, string> elementToString, bool firstElementInGroupInline =false)
        {
            if (input == null)
                return null;

            var outputString = new StringBuilder();

            foreach (var group in input)
            {
                if (outputString.Length == 0)
                    outputString.Append(groupKeyToString(@group.Key));
                else
                    outputString.Append(groupConcatinator).Append(groupKeyToString(@group.Key));

                outputString.Append(@group.ToString(elementConcatinator, elementToString, firstElementInGroupInline));
            }

            return outputString.ToString();
        }

        /// <summary>
        /// Дистинкт с лямбда-выражением.
        /// !!!Использовать только с локальными коллекциями!!!
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source">Объект <see>
        ///                                 <cref>T:System.Linq.IQueryable`1</cref>
        ///                             </see>
        ///     , из которого требуется удалить дубликаты.</param>
        /// <param name="keySelector"></param>
        /// <returns> </returns>
        /// <exception cref="T:System.ArgumentNullException"> Параметр <paramref name="source"/> или <paramref name="keySelector"/> имеет значение null.</exception>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            var knownKeys = new HashSet<TKey>();

            return source.Where(element => knownKeys.Add(keySelector(element))).ToList();
        }
    }
}
