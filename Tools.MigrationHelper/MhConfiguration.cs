using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper
{
	public class MhConfiguration
	{
		// параметры
		public int DevId = -1;
		public string Action = string.Empty;
		public string ConnectionString;
		public string SourcePath;
		public string TargetPath;
		public bool Verbose;
		public bool GenerateCode;
		/// <summary>
		/// Путь куда выгружается дистрибутив
		/// </summary>
		public string OutputDir;

		/// <summary>
		/// При действии ToFs удалять xml-классы, которые не были изменены 
		/// </summary>
		public bool DeleteUntouchedXml = true;
	}
}
