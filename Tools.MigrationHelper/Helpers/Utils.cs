using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Helpers
{
	public class Utils
	{
		/// <summary>
		/// Получить имя схемы по типу сущности
		/// </summary>
		public static string getSchemaByEntityType(int entityType)
		{
			switch (entityType)
			{
				case 1:
					return "enm";
				case 3:
					return "ref";
				case 4:
					return "tp";
				case 5:
					return "ml";
				case 6:
					return "doc";
				case 7:
					return "tool";
				case 8:
					return "reg";
				case 9:
					return "rep";
			}
			return null;
		}
	}
}
