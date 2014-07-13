using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.PrimaryEntities
{
	/// <summary>
	/// Базовый класс для любой сущности
	/// </summary>
	public abstract class BaseEntity : IBaseEntity
	{
		#region Реализация IBaseEntity
		public virtual int EntityId { get { return -1; } }

		public virtual string EntityCaption
		{
			get { return ""; }
		}

		/// <summary>
		/// Сформировать объект сущноси по строке из таблицы
		/// </summary>
		/// <param name="row"></param>
		public void FromDataRow(DataRow row)
		{
			Dictionary<string, PropertyInfo> props = _getProperties();

			foreach (DataColumn col in row.Table.Columns)
			{
				string name = col.ColumnName.ToLower();
				if (props.ContainsKey(name))
					_setValue(props[name], row[name]);
			}
		}

		/// <summary>
		/// Получить объект из записи SqlDataReader'а
		/// </summary>
		/// <param name="record">Запись</param>
		public void FromDataRecord(IDataRecord record)
		{
			Dictionary<string, PropertyInfo> props = _getProperties();

			for (int i = 0; i < record.FieldCount; i++)
			{
				var name = record.GetName(i).ToLower();
				if (props.ContainsKey(name))
				{
					_setValue(props[name], record.GetValue(i));
				}
			}
		}

	    public void FromDictionary(IDictionary<string, object> values)
		{
			Dictionary<string, PropertyInfo> props = _getProperties();

            foreach (KeyValuePair<string, object> pair in values)
            {
                string name = pair.Key.ToLower();
                if (props.ContainsKey(name))
	                _setValue(props[name], pair.Value);
            }
	    }
		#endregion

		/// <summary>
		/// Получение значений обекта в виде словаря
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, object> ToDictionary()
		{
			return _toDictionary(_getProperties());
		}

		/// <summary>
		/// Получение значений обекта в виде словаря
		/// </summary>
		/// <param name="flags">Фильтр</param>
		/// <returns></returns>
		public IDictionary<string, object> ToDictionary(BindingFlags flags)
		{
			return _toDictionary(_getProperties(flags));
		}

		/// <summary>
		/// Получение значений обекта в виде словаря для указаный свойств
		/// </summary>
		/// <param name="properties">Список свойств</param>
		/// <returns></returns>
		private IDictionary<string, object> _toDictionary(IDictionary<string, PropertyInfo> properties)
		{
			var values = new Dictionary<string, object>();
			foreach (KeyValuePair<string, PropertyInfo> pair in properties)
			{
				string name = pair.Key.ToLower();
				values.Add(name, _getValue(pair.Value));
			}
			return values;
		}

		/// <summary>
		/// Получение свойств объекта в виде словаря
		/// </summary>
		/// <param name="flags">Фильтр</param>
		/// <returns></returns>
		private Dictionary<string, PropertyInfo> _getProperties(BindingFlags flags)
		{
			return _getProperties(GetType().GetProperties(flags));
		}

		/// <summary>
		/// Получение свойств типа в виде словаря
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, PropertyInfo> _getProperties()
		{
			return _getProperties(GetType().GetProperties());
		}

		/// <summary>
		/// Получить свойства данного объекта в виде словаря
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, PropertyInfo> _getProperties(IEnumerable<PropertyInfo> properties)
		{
			return properties.ToDictionary(a => a.Name.ToLower(), b => b);
		}

		/// <summary>
		/// Установить значение <paramref name="value"/> в свойство <paramref name="prop"/> для объекта this.
		/// </summary>
		private void _setValue(PropertyInfo prop, object value)
		{
			if (value != DBNull.Value
				&& prop.CanWrite
				&& prop.GetSetMethod(/*nonPublic*/ true).IsPublic)
			{
				if (value == null || prop.PropertyType.IsInstanceOfType(value))
					prop.SetValue(this, value, null);
				else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
					prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType.GetGenericArguments()[0]), null);
				else
					prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType), null);
			}
		}

		/// <summary>
		/// Получить значение из свойства <paramref name="prop"/> объекта this.
		/// </summary>
		private object _getValue(PropertyInfo prop)
		{
			if (prop.CanRead && prop.GetGetMethod(/*nonPublic*/ true).IsPublic)
			{
				return prop.GetValue(this, null);
			}
			return null;
		}

        /// <summary>
		/// Получение наименования обекта в виде "EntityCaption : Id"
        /// </summary>
        /// <returns></returns>
		public override string ToString()
        {
            var identitied = this as IIdentitied;
            if (identitied != null)
                return String.Format("{0} : {1}", EntityCaption, identitied.Id);
            
            return base.ToString();
        }

	    /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IBaseEntity other)
        {
            if (!(this is IIdentitied))
                return ReferenceEquals(this, other);
            if (other == null)
                return false;
            if (this.GetType() != other.GetType())
                return false;
            if (((IIdentitied)this).Id == 0)
                return ReferenceEquals(this, other);
            return ((IIdentitied)this).Id == ((IIdentitied)other).Id;
        }
	}
}
