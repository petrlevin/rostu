using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.MigrationHelper.DbManager;

namespace Tools.MigrationHelper.Tasks
{
	/// <summary>
	/// Импорт метаданных из базы в xml файлы
	/// </summary>
	class ToFs : MhTaskBase
	{
		public override int Process(MhConfiguration config, System.IO.TextWriter output)
		{
			base.Process(config, output);
			var metadata = new Metadata();
			metadata.FromDb(_config.ConnectionString);
			metadata.ToFs(_config.SourcePath, _config.DeleteUntouchedXml);
			if (_config.GenerateCode)
			{
				var generator = new Generator.Entities.CodeGenerator(_config.SourcePath, metadata.DataSet);
				generator.Generate();
			}
			return Success;
		}
	}
}
