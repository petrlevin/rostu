using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Denormalizer
{
	public abstract class PeriodsProviderBase
	{
	    public PeriodsProviderBase()
	    {
		    DataContext db = (DataContext)IoC.Resolve<DbContext>();
			entityFieldProps = db.Entity.Single(e => e.Name == "EntityField").RealFields;
	    }

	    protected IDictionary<string, object> toDictionary(IEntityField field)
		{
			var result = new Dictionary<string, object>();
			List<string> realFileds = entityFieldProps.Select(f => f.Name.ToLower()).ToList();
			Dictionary<string, PropertyInfo> fieldProperties = getProperties(field);
			foreach (string realFiled in realFileds)
			{
				if (fieldProperties.ContainsKey(realFiled))
				{
					var value = getValue(field, fieldProperties[realFiled]);
					result[realFiled] = value;
				}
			}
			return result;
		}

		
		
		/// <summary>
		/// Свойства любого поля сущности
		/// </summary>
		private IEnumerable<IEntityField> entityFieldProps { get; set; }

		/// <summary>
		/// Получить свойства данного объекта в виде словаря
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, PropertyInfo> getProperties(object field)
		{
			var result = new Dictionary<string, PropertyInfo>();
			foreach (PropertyInfo p in field.GetType().GetProperties())
				result.Add(p.Name.ToLower(), p);
			return result;
		}

		/// <summary>
		/// Получить значение из свойства <paramref name="prop"/> объекта this.
		/// </summary>
		private object getValue(object obj, PropertyInfo prop)
		{
			if (prop.CanRead && prop.GetGetMethod(/*nonPublic*/ true).IsPublic)
			{
				return prop.GetValue(obj, null);
			}
			return null;
		}
	}
}