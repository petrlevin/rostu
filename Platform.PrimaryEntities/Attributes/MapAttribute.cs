using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class MapAttribute : Attribute
	{
		///<summary>
		/// Поле которое ссылается
		///</summary>
		public string ForeignKey { get; set; }

		///<summary>
		/// Поле на которое ссылаются
		///</summary>
		public string PrimaryKey { get; set; }
	}
}
