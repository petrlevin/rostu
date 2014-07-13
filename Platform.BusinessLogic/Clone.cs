using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic
{
	/// <summary>
	/// Класс реализующий механизм клонирование элемента вместе с его подчиненными ТЧ и МЛ
	/// </summary>
	public class Clone
	{
		/*TODO
		 * Выделить в словари поля ТЧ и проперти ТЧ. В коде клонирования ТЧ не должно быть 
		 * кода получения полей сущности и свойств сущностного класса.
		 * Добавить конструктор со списком исключаемых из копирования полей
		 * Объеденить _cloneTpItem и _cloneItemFromTpById
*/
		/// <summary>
		/// Клонируемая сущность
		/// </summary>
		private readonly Entity _entity;

		/// <summary>
		/// Элемент клонируемой сущности
		/// </summary>
		private readonly object _source;

		/// <summary>
		/// Результирующий объект
		/// </summary>
		private object _target;

		/// <summary>
		/// Поля с типом ТЧ из клонируемой сущности
		/// </summary>
		private readonly List<IEntityField> _entityFieldsTp;

		/// <summary>
		/// Сущности табличных частей клонируемой сущности
		/// </summary>
		private readonly List<IEntity> _entitiesTp;

		/// <summary>
		/// Клонированные элементы табличных частей(ключ - идентификатор исходного элемента_идентификатор сущности, значение - клон)
		/// </summary>
		private Dictionary<string, object> _clonedTpItems = new Dictionary<string, object>();


		/// <summary>
		/// Получение названия свойства мультилинка на основнии поля сущности
		/// </summary>
		/// <param name="mlField">Поле сущности</param>
		/// <returns></returns>
		private static string _multilinkPropertyName(IEntityField mlField)
		{
			return Regex.Replace(mlField.Name, "^ml", "", RegexOptions.IgnoreCase).FirstUpper();
		}

		/// <summary>
		/// Получение названия свойства ТЧ на основнии поля сущности
		/// </summary>
		/// <param name="mlField">Поле сущности</param>
		/// <returns></returns>
		private static string _tablePartPropertyName(IEntityField mlField)
		{
			return Regex.Replace(mlField.Name, "^tp", "", RegexOptions.IgnoreCase).FirstUpper();
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="source">Элемент клонируемой сущности</param>
		public Clone(object source)
		{
			if ((source as BaseEntity)==null)
				throw new Exception("Клонирование реализовано только для наследников от BaseEntity");
			_source = source;
			_entity = Objects.ById<Entity>((source as BaseEntity).EntityId);
			_target = Activator.CreateInstance(source.GetType());
			_entityFieldsTp = _entity.Fields.Where(
				a => a.EntityFieldType == EntityFieldType.Tablepart && a.IdEntityLink.HasValue).ToList();
			_entitiesTp = _entityFieldsTp.Select(a => a.EntityLink).ToList();
		}

		/// <summary>
		/// Возвращает клонированный объект
		/// </summary>
		/// <returns></returns>
		public object GetResult()
		{
			_cloneBaseObject();
			_cloneAllTp();
			_cloneAllMl();
			return _target;
		}

		/// <summary>
		/// Клонирование самого объекта(физически существующих в таблице полей)
		/// </summary>
		private void _cloneBaseObject()
		{
			List<string> fields = _entity.RealFields.Where(a => !a.IdCalculatedFieldType.HasValue && a.Name != "id").Select(a => a.Name).ToList();
			PropertyInfo[] propertyInfos = _source.GetType().GetProperties().Where(a => fields.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).ToArray();
			_target = _cloneItem(_source, propertyInfos);
			/*foreach (PropertyInfo property in propertyInfos)
			{
				_target.SetValue(property.Name, _source.GetValue(property.Name));
			}*/
		}

		/// <summary>
		/// Клонирование всех мультилинков
		/// </summary>
		private void _cloneAllMl()
		{
			List<EntityField> fieldsMl =
				_entity.Fields.Cast<EntityField>().Where(
					a => a.EntityFieldType == EntityFieldType.Multilink && a.IdEntityLink.HasValue).ToList();
			foreach (EntityField entityField in fieldsMl)
			{
				//if (entityField.EntityLink)
				//TODO сделать проверку на сортированный мультилинк и выдать исключение
 				//после реализации маппинга упорядоченных мультиссылок сделать реализацию
				string propertyName = _multilinkPropertyName(entityField);
				IEnumerable source = (IEnumerable)_source.GetValue(propertyName);
				if (source != null && (source as IList).Count > 0)
				{
					IEnumerable target = (IEnumerable)_target.GetValue(propertyName);
					PropertyInfo propertyInfo = _source.GetType().GetProperty(propertyName);
					foreach (var item in source)
					{
						propertyInfo.PropertyType.GetMethod("Add").Invoke(target, new[] { item });
					}
				}
			}
		}

		/// <summary>
		/// Клонирование всех ТЧ
		/// </summary>
		private void _cloneAllTp()
		{
			foreach (IEntityField entityField in _entityFieldsTp)
			{
				string propertyName = _tablePartPropertyName(entityField);
				PropertyInfo propertyInfo = _source.GetType().GetProperty(propertyName);
				if (propertyInfo != null)
				{
					List<string> excludedField = new List<string> { "id", "idOwner" };
					IEnumerable source = (IEnumerable)_source.GetValue(propertyName);
					IEnumerable target = (IEnumerable)_target.GetValue(propertyName);
					_cloneTp(source, target, propertyInfo, entityField.EntityLink, excludedField);
				}
			}
		}

		/// <summary>
		/// Клонирование ТЧ
		/// </summary>
		/// <param name="source">Элементы клонируемой ТЧ</param>
		/// <param name="target">Результирующие элементы</param>
		/// <param name="propertyInfo">Свойство класса соответствущей ассоции на ТЧ</param>
		/// <param name="entity">Сущность клонируемой ТЧ</param>
		/// <param name="excludedField">Поля ТЧ исключаемые из клонирования</param>
		private void _cloneTp(IEnumerable source, IEnumerable target, PropertyInfo propertyInfo, IEntity entity, IEnumerable<string> excludedField)
		{
			List<IEntityField> listFields =
				entity.RealFields.Where(
					a => !a.IdCalculatedFieldType.HasValue && !excludedField.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).ToList();
			List<string> listNameFields = listFields.Select(a => a.Name).ToList();

			List<PropertyInfo> copyProperties = null;
			foreach (var item in source)
			{
				if (copyProperties == null)
					copyProperties =
						item.GetType().GetProperties().Where(a => listNameFields.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).ToList();
				object newItem = _cloneTpItem(item, copyProperties, listFields);
				propertyInfo.PropertyType.GetMethod("Add").Invoke(target, new[] {newItem});
			}
		}

		/// <summary>
		/// Клонирование произвольного объекта
		/// </summary>
		/// <param name="item">Источник для конирования</param>
		/// <param name="propertyInfos">Список свойств, которые будут клоинрованы</param>
		/// <returns></returns>
		private object _cloneItem(object item, IEnumerable<PropertyInfo> propertyInfos)
		{
			object result = Activator.CreateInstance(item.GetType());
			foreach (PropertyInfo property in propertyInfos)
			{
				result.SetValue(property.Name, item.GetValue(property.Name));
			}
			return result;
		}

		/// <summary>
		/// Получить элемент из списка уже скопированных
		/// </summary>
		/// <param name="id">Идентификатор элемента источника</param>
		/// <param name="idEntity">Идентификатор сущности элемента</param>
		/// <returns>Клон объекта</returns>
		private object _getFromCloned(int id, int idEntity)
		{
			object result = null;
			_clonedTpItems.TryGetValue(string.Format("{0}{1}", id, idEntity), out result);
			return result;
		}

		/// <summary>
		/// Добавить элемент в список уже скопированных
		/// </summary>
		/// <param name="id">Идентификатор элемента источника</param>
		/// <param name="idEntity">Идентификатор сущности элемента</param>
		/// <param name="item">Добавляемый элемент</param>
		/// <returns>Клон объекта</returns>
		private void _addToCloned(int id, int idEntity, object item)
		{
			if (!_clonedTpItems.ContainsKey(string.Format("{0}{1}", id, idEntity)))
				_clonedTpItems.Add(string.Format("{0}{1}", id, idEntity), item);
		}
		
		/// <summary>
		/// Клонирование строки ТЧ
		/// </summary>
		/// <param name="item">Источник для копирования</param>
		/// <param name="propertyInfos">Список свойств, которые будут скопированы</param>
		/// <param name="listFields">Список полей</param>
		/// <returns></returns>
		/// <remarks>Алгоритм испольует рекурсию при обнаружении ссылок на элементы ТЧ клонируемой сущности.
		/// Каждый клонированная строка записывается в <see cref="_clonedTpItems">словарь</see></remarks>
		private object _cloneTpItem(object item, IEnumerable<PropertyInfo> propertyInfos, List<IEntityField> listFields)
		{
			int id = (int)item.GetValue("Id");
			int idEntity = listFields.First().IdEntity;
			object result= _getFromCloned(id, idEntity);
			if (result != null)
				return result;
			result = Activator.CreateInstance(item.GetType());
			foreach (PropertyInfo property in propertyInfos)
			{
				IEntityField entityField = listFields.Single(a => a.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
				if (entityField.EntityFieldType == EntityFieldType.Link 
					&& entityField.EntityLink.EntityType==EntityType.Tablepart 
					&& _entitiesTp.Any(a => a.Id == entityField.IdEntityLink))
				{
					IEntityField entityFieldTp = _entityFieldsTp.Single(a => a.IdEntityLink == entityField.IdEntityLink);
					int? idItem = (int?) item.GetValue(property.Name);
					if (idItem.HasValue)
					{
						object newObject = _cloneItemFromTpById(entityFieldTp, idItem.Value);
						result.SetValue(property.Name.Substring(2), newObject);
					} else
					{
						result.SetValue(property.Name, null);
					}
				}
				else
				{
					result.SetValue(property.Name, item.GetValue(property.Name));
				}
			}
			_addToCloned(id, idEntity, result);
			return result;
		}

		/// <summary>
		/// Клонирование элемента с определенным идентификатором из указанной ТЧ 
		/// </summary>
		/// <param name="entityFieldTp">Поле сущности с типом ТЧ</param>
		/// <param name="id">Идентификатор элемента источника</param>
		/// <returns></returns>
		private object _cloneItemFromTpById(IEntityField entityFieldTp, int id)
		{
			if (!entityFieldTp.IdEntityLink.HasValue)
				throw new PlatformException("Ошибка клонирования");

			int idEntity = entityFieldTp.IdEntityLink.Value;
			object result = _getFromCloned(id, idEntity);
			if (result != null)
				return result;

			string propertyName = _tablePartPropertyName(entityFieldTp);
			IEnumerable source = (IEnumerable) _source.GetValue(propertyName);
			object target = _target.GetValue(propertyName);
			object copyObject = source.Cast<object>().FirstOrDefault(item => (int) item.GetValue("Id") == id);
			if (copyObject == null)
				throw new PlatformException("Ошибка клонирования");

			List<string> excludedField = new List<string> {"id", "idOwner"};

			List<IEntityField> listFields =
				entityFieldTp.EntityLink.RealFields.Where(
					a => !a.IdCalculatedFieldType.HasValue && !excludedField.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).
					ToList();

			List<string> listNameFields = listFields.Select(a => a.Name).ToList();

			List<PropertyInfo> copyProperties =
				copyObject.GetType().GetProperties().Where(a => listNameFields.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).
					ToList();

			result = _cloneTpItem(copyObject, copyProperties, listFields);
			if (result == null)
				throw new Exception("Ошибка клонирования");
			_addToCloned(id, idEntity, result);

			PropertyInfo propertyInfo = _target.GetType().GetProperty(propertyName);
			propertyInfo.PropertyType.GetMethod("Add").Invoke(target, new[] {result});
			return result;
		}

	}
}
