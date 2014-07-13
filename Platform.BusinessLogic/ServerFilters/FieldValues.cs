using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.BusinessLogic.ServerFilters
{
	/// <summary>
	/// Класс описывающий значения для полей, от которых зависит поле, данные (форма выбора или грид) для которого запрошены с клиента. 
	/// Идентификатор поля - значение.
	/// </summary>
	public class FieldValues : Dictionary<int, object>
	{
		/// <summary>
		/// Получение, добавление, изменение элемента по ключу
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public new object this[int key]
		{
			get
			{
				return ContainsKey(key) ? base[key] : null;
			}
			protected set
			{
				// для упрощения процесса создания новых полей 
				// повесим эту функцию на оператор присваивания
				if (!ContainsKey(key))
					Add(key, value);
				base[key] = value;
			}
		}

        /// <summary>
        /// Создание нового экземпляра из строки
        /// </summary>
        /// <param name="fieldValues"></param>
        /// <returns></returns>
        public static FieldValues FromString(string fieldValues)
        {
            var result = new FieldValues();

            if (string.IsNullOrEmpty(fieldValues))
                return null;

            var pairs = Regex.Matches(fieldValues, @".*?(?<key>[-|\w]+).*?(?<value>[-|\w]+)", RegexOptions.Singleline);
            foreach (Match pair in pairs)
            {
                int key = int.Parse(pair.Groups["key"].Value);
                object value = pair.Groups["value"].Value;
                result.Add(key, value);
            }
            return result;
        }
	}
}
