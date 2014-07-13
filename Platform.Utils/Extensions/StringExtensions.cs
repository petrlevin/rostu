using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Utils.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Делает у строки первую букву заглавной
		/// </summary>
		/// <param name="value">Обрабатываемая строка</param>
		/// <returns></returns>
		public static string FirstUpper(this string value)
		{
			if (value.Length == 0)
				return value;
			if (value.Length == 1)
				return value.Substring(0, 1).ToUpperInvariant();
			if (value.Length > 1)
				return value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
			
			return value;
		}

		///<summary>
		/// Приводет объекты к строке проверяя на null
		///</summary>
		///<param name="o">любой объект</param>
		///<returns>приведенный объект</returns>
		public static string NullableToString(this object o)
		{
			return NullableToString(o, string.Empty);
		}

		public static string NullableToString(this object o, string nullReplacer)
		{
			return o == null ? nullReplacer : o.ToString();

		}

        ///// <summary>
        ///// Соединение перечисления в строку
        ///// </summary>
        ///// <typeparam name="T">Тип перечисляемых элементов</typeparam>
        ///// <param name="input">Перечисление элементов</param>
        ///// <param name="concatinator">Строка используемая для соединения</param>
        ///// <param name="func">преобразование элемента перечисления в строку</param>
        ///// <returns>Соединенная строка</returns>
        //public static string ToString<T>(this IEnumerable<T> input, string concatinator, Func<T, string> func)
        //{
        //    if (input == null)
        //        return null;
        //    var sb = new StringBuilder();

        //    var enumer = input.GetEnumerator();
        //    var moved = enumer.MoveNext();
        //    if (moved)
        //    {
        //        sb.Append(func(enumer.Current));

        //        while (moved)
        //        {
        //            moved = enumer.MoveNext();
        //            if (moved)
        //            {
        //                sb.Append(concatinator);
        //                sb.Append(func(enumer.Current));
        //            }
        //        }
        //    }

        //    return sb.ToString();
        //}
	}
}
