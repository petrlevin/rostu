using System;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.DbManager;

namespace Tools.MigrationHelper.Core.Tasks
{
	/// <summary>
	/// Импорт метаданных из базы в xml файлы
	/// </summary>
   [TaskName("tofs")]
	public class ToFs : DbDeployTask
	{

       [TaskAttribute("deleteuntouchedxml", Required = false)]
       public bool DeleteUntouchedXml
       {
           get; set;
       }

	   protected override void ExecuteTask()
	    {
	        try
	        {
				var metadata = new DbDataSet(DevId, ConnectionString);
                Log(Level.Verbose, "Получение метаданных из БД");
                metadata.FillDependencies();
                metadata.FromDb(ConnectionString);
                Log(Level.Verbose, "Выгрузка метаданных в папку ",SourcePath);
                metadata.ToFs(SourcePath, DeleteUntouchedXml);
	        }
	        catch (BuildException)
	        {
	            throw;
	        }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при импорте метаданных из базы в xml файлы", ex);
            }

	    }
	}
}
