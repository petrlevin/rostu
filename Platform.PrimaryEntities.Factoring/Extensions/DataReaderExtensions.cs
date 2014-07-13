using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;

namespace Platform.PrimaryEntities.Factoring.Extensions
{
	public static class DataReaderExtensions
	{
		private static T FromRecord<T>(IDataRecord record) where T : IBaseEntity, new()
		{
			var t = new T();
			t.FromDataRecord(record);
			return t;
		}

		public static IEnumerable<T> AsEnumerable<T>(this IDataReader reader) where T : IBaseEntity, new()
		{
            return Platform.Utils.Extensions.DbDataReaderExtension.AsEnumerable(reader,FromRecord<T>);
		}

	}
}
