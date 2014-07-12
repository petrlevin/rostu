using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using Platform.PrimaryEntities.Attributes;

namespace Tools.MigrationHelper.EntitySerializer
{
	/// <summary>
	/// Класс реализующий десериализацию из xml в экзепляры классов (CORE-4)
	/// </summary>
	public static class Deserialazer
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="dir">Путь до папки с файлами xml</param>
		/// <param name="withCollections"></param>
		/// <returns></returns>
		public static IEnumerable<T> DeserialiseXmlFromDir<T>(this IEnumerable<T> data, string dir, bool withCollections)
		{
			if(string.IsNullOrEmpty(dir))
				throw new Exception("Не указан путь до папки с файлами xml");
			var dic = new Dictionary<string, string>();
			var files = Directory.GetFiles(dir,"*.xml");
			if(!files.Any())
				throw new Exception("В папке не найдено ни одного файла с расширением xml");
			foreach (var file in files)
			{
				var stream = new StreamReader(file);
				var fName = Path.GetFileNameWithoutExtension(file);
				if (!dic.ContainsKey(fName))
					dic.Add(fName, stream.ReadToEnd());
			}
			return DeserialiseXml(data, dic, withCollections);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="file">путь до файла</param>
		/// <returns></returns>
		public static IEnumerable<T> DeserialiseXmlFromFile<T>(this IEnumerable<T> data, string file)
		{
			if (string.IsNullOrEmpty(file))
				throw new Exception("Не указан путь до файла");
			if(Path.GetExtension(file) != ".xml")
				throw new Exception("Файл не имеет расширение xml");

			

			var dic = new Dictionary<string, string>();
			var stream = new StreamReader(file);
			
			dic.Add(Path.GetFileNameWithoutExtension(file), stream.ReadToEnd());
			return DeserialiseXml(data, dic, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="xml"> Набор Имя сущности = текст xml </param>
		/// <param name="withCollections">Заполнять коллекции</param>
		/// <returns></returns>
		public static IEnumerable<T> DeserialiseXml<T>(this IEnumerable<T> data, Dictionary<string, string> xml, bool withCollections)
		{
			if(xml == null || !xml.Any())
				throw new Exception("Отсутствует словарь с xml");

			// получаем коллекцию родителей
			data = Deserialize(data, typeof(T), xml);
			
			var fields = GetCollectionsFields(typeof(T)); // получаем имя дочерней коллекции и связку 
			//заполняем коллекции
			if (fields.Any() && xml.Count > 1 && data.Any() && withCollections) // если есть дочерние коллекции и есть ли в словаре еще что нибудь
			{
				foreach (var field in fields)
				{
					var fieldType = field.Property.PropertyType.GetGenericArguments()[0];
					dynamic result = GetListOfT(fieldType);

					//Вызываем самого себя для заполнения дочерних коллекций
					IQueryable query = Queryable.AsQueryable(DeserialiseXml(result, xml, true));

					//Создание динамического linq запроса
					//i=>
					ParameterExpression parameter = Expression.Parameter(fieldType, "i");
					//i.ForeignKey
					MemberExpression property = Expression.Property(parameter, field.ControlAttribute.ForeignKey);

					foreach (var d in data)
					{
						IQueryable value = query.Provider.CreateQuery(GetQuery(field.ControlAttribute.PrimaryKey, d, query, parameter, property));
						field.Property.SetValue(d, value);
					}
				}
			}
			return data;
		}

		/// <summary>
		/// Десериализация из xml в экземпляр класса для данного Типа
		/// </summary>
		/// <param name="dataList"></param>
		/// <param name="type">Тип</param>
		/// <param name="xml">Словарь с xml</param>
		private static dynamic Deserialize(dynamic dataList, Type type, IReadOnlyDictionary<string, string> xml)
		{
			if(!xml.ContainsKey(type.Name))
				throw new Exception("Отсутствует xml файл для типа: " + type.Name);
			var item = xml[type.Name];
			var serializer = new XmlSerializer(dataList.GetType(), new XmlRootAttribute("root"));
			using (TextReader reader = new StringReader(item))
			{
				return serializer.Deserialize(reader);
			}
		}

		/// <summary>
		/// Создание динамического запроса linq
		/// </summary>
		/// <param name="primaryKey">Первичный ключ родителя</param>
		/// <param name="parent">Значение по которому фильтруем</param>
		/// <param name="query">Коллекция для которой пишем Where</param>
		/// <param name="parameter">i=></param>
		/// <param name="property">i.ForeignKey</param>
		/// <returns></returns>
		private static MethodCallExpression GetQuery(string primaryKey, object parent, IQueryable query, ParameterExpression parameter, MemberExpression property)
		{
			//i.ForeignKey == parent.PrimaryKey
			var condition = Expression.Equal(property, GetConstant(parent, primaryKey, property.Type));
			//i => i.ForeignKey == parent.PrimaryKey
			var lambda = Expression.Lambda(condition, new[] { parameter });

			return Expression.Call(
				typeof(Queryable),
				"Where",
				new[] { query.ElementType },
				query.Expression,
				lambda);
		}

		private static ConstantExpression GetConstant(object parent, string primaryKey, Type T)
		{
			if (string.IsNullOrEmpty(primaryKey))
				throw new Exception("Отсутствует значение атрибута primaryKey");

			PropertyInfo[] props = parent.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var firstOrDefault = props.FirstOrDefault(w => w.Name == primaryKey);
			if (firstOrDefault == null)
				throw new Exception("Отсутствует свойство у сущности");
			ConstantExpression constant = Expression.Constant(Convert.ChangeType(firstOrDefault.GetValue(parent), T));
			return constant;
		}

		private static ControlDescriptor[] GetCollectionsFields(Type t)
		{
			return GetInfoAboutCollections(t).Select(pi => new ControlDescriptor { Property = pi, ControlAttribute = pi.GetCustomAttributes(typeof(MapAttribute), false).Cast<MapAttribute>().Single() }).ToArray();
		}

		private static IEnumerable<PropertyInfo> GetInfoAboutCollections(Type t)
		{
			return (from propertyInfo in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					where propertyInfo.GetCustomAttributes(typeof(MapAttribute), false).Any()
					select propertyInfo).ToArray();
		}

		/// <summary>
		/// Возвращает List передаваемого типа
		/// </summary>
		/// <param name="T">Тип</param>
		/// <returns></returns>
		public static object GetListOfT(Type T)
		{
			return Activator.CreateInstance(typeof(List<>).MakeGenericType(T));
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	class ControlDescriptor
	{
		/// <summary>
		/// 
		/// </summary>
		public PropertyInfo Property { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public MapAttribute ControlAttribute { get; set; }
	}
}
