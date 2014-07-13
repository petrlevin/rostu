using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Platform.Client
{
	/// <summary>
	/// Описание поля подлежащего сохранению
	/// </summary>
	public class SaveElement
	{
		/// <summary>
		/// Ключ - название поля
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Значение/содержимое поля
		/// </summary>
		public object Value { get; set; }
 
		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Platform.Client.Common.SaveElement"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:Platform.Client.Common.SaveElement"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Key: {0}, Value: {1}", this.Key, this.Value);
		}
	}
}
