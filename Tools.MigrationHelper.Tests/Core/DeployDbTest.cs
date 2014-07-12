using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;
using NAnt.Core;
using NUnit.Framework;
using Platform.PrimaryEntities.DbEnums;
using System.IO;
using Tools.MigrationHelper.Helpers;
using SharpSvn;

namespace Tools.MigrationHelper.Tests.Core
{
	/// <summary>
	/// Тест на развертывание БД (CORE-43).
	/// </summary>
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class DeployDbTest :BuildTestBase
	{
		private SqlConnection _connection;

		/// <summary>
		/// Количество всех таблиц в БД
		/// </summary>
		private static string sqlCntTables = "SELECT COUNT(*) FROM information_schema.tables WHERE table_type = 'base table'";

		/// <summary>
		/// Количество таблиц перечислений (из сзхемы enm) в БД 
		/// </summary>
		private static string sqlCntEnumTables = sqlCntTables + " AND TABLE_SCHEMA = 'enm'";

		/// <summary>
		/// Количество справочников
		/// </summary>
		private static string sqlCntReference = "SELECT COUNT(*) FROM ref.Entity WHERE idProject = 100 AND idEntityType <> 1";

		[SetUp]
		protected new void SetUp()
		{
			// соединение с БД
			_connection = new SqlConnection(connectionString);
		}

		/// <summary>
		/// Осторожно! в конце теста делается REVERT папок DbEnums, DbStructure и файлов проекта.
		/// </summary>
		/// <remarks>
		/// Без реверта возникает проблема что свн помечает эти файлы как измененые или удаленные и они выпадают из проекта
		/// </remarks>
		[Test]
        [Ignore]
		public void Run()
		{
		    try
		    {
                deployPlatformDb();
                deployPlatformDb_ToFs();
                DeployAppDb();
                DeployAppDb_ToFs();
		    }
		    finally 
		    {
                revertDbStructure();
		    }
		}

		/// <summary>
		/// Проверка консистентности ИБ
		/// </summary>
		[Test]
		public void ConsistencyCheck()
		{
            var project = ExucuteConnectStringMH("check");
            Assert.AreEqual("true", project.Properties["check.result"]);
		}

		#region Test Steps

		/// <summary>
		/// DeployPlatformDb. 
		/// Результат: Разворачивается платформенная БД. 
		/// Проверить наличие конкретных таблиц в БД (по именам) и конкретных полей в таблицах (по именам). 
		/// Если будет присутствовать больше таблиц и/или больше полей - тест прошел, но следует выдать warning (или просто сообщение) о том, 
		/// что в тест нужно доработь и учесть вновь созданные поля/таблицы.
		/// </summary>
		private void deployPlatformDb()
		{
			ExucuteDeployMH("DeployPlatformDb");

			int tablesCount = ExecuteScalar<int>(sqlCntTables);
			Assert.AreEqual(
				GetCountFromXml(Path.Combine(solutionPath, @"Tools.MigrationHelper.Core\DbStructure\ref\Entity.xml"), "//ref.Entity")
				+ GetCountFromXml(Path.Combine(solutionPath, @"Tools.MigrationHelper.Core\DbEnums\Entity.xml"), "//ref.Entity"),
				tablesCount);
		}

		/// <summary>
		/// ToFs. Результат: В Tools.MigrationHelper\DbStructure\ref будут созданы 2 файла Entity.xml и EntityField.xml. 
		/// а) проверить наличие 
		/// б) проверить количество узлов - должно быть не меньше чем сущностей/полей. 
		/// Если равно - тест прошел. Если в xml узлов меньше - тест не прошел. 
		/// Если в xml узлов больше - вывести warning о необходимости актуализировать тест.
		/// </summary>
		private void deployPlatformDb_ToFs()
		{
			ExucuteDeployMH("ToFs");

			int referenceCount = ExecuteScalar<int>(sqlCntReference);
			var path = Path.Combine(solutionPath, @"Tools.MigrationHelper.Core\DbStructure\ref\Entity.xml");
			Assert.IsTrue(File.Exists(path));
			Assert.AreEqual(GetCountFromXml(path, "//ref.Entity"), referenceCount);
			// ToDo: Где проверка на количество полей?
		}

		/// <summary>
		/// DeployAppDb. Результат: В БД будут развернуты перечисления. 
		/// Проверить наличие таблиц в пространстве имен enm и состав полей каждой таблицы.
		/// </summary>
        private void DeployAppDb()
		{
			ExucuteDeployMH("DeployAppDb");

			int enumTableCount = ExecuteScalar<int>(sqlCntEnumTables);
			var enums = new EnumsProcessing.EnumsFetcher();
			var enumsList = enums.GetEnums(solutionPath, null, false, null);

			Assert.AreEqual(enumsList.Count, enumTableCount);
			// ToDo: Где проверка состава полей таблиц перечислений?
		}

		/// <summary>
		/// ToFs. Результат: В каждом проекте, где существует каталог DbEnums, 
		/// в данную директорию запишутся Entity.xml и EntityField.xml, проверить наличие файлов.
		/// </summary>
		private void DeployAppDb_ToFs()
		{
			ExucuteDeployMH("ToFs");

			Assert.True(File.Exists(Path.Combine(solutionPath, @"Platform.PrimaryEntities\DbEnums\Entity.xml")));
			Assert.True(File.Exists(Path.Combine(solutionPath, @"Platform.PrimaryEntities\DbEnums\EntityField.xml")));
			// ToDo: Здесь проверка в конкретных проектах, а нужно пройтись по проектам из SolutionProject
		}

		/// <summary>
		/// Делаем реверт папок с xml и файлов проекта
		/// </summary>
		private void revertDbStructure()
		{
			foreach (var project in Enum.GetNames(typeof(SolutionProject)))
			{
				var projectPath = new DirectoryInfo(Path.Combine(solutionPath, project.Replace('_', '.')));
				RevertFiles(projectPath);
				using (var svnClient = new SvnClient())
				{
					svnClient.Revert(Path.Combine(projectPath.FullName, projectPath.Name + ".csproj"), new SvnRevertArgs { Depth = SvnDepth.Infinity });
				}
			}
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Получение количества корневых узлов в xml файле
		/// </summary>
		/// <param name="path">Путь до файла xml</param>
		/// <param name="select">Имя узла(если не корневой указывается с //)</param>
		/// <returns></returns>
		private int GetCountFromXml(string path, string select)
		{
			var doc = new XmlDocument();
			var reader = new StreamReader(path);
			doc.Load(reader);
			var xmlNodeList = doc.SelectNodes(select);
			return xmlNodeList != null ? xmlNodeList.Count : 0;
		}


        private Project ExucuteConnectStringMH(string action)
        {
            string xml = @"
                    <project>
                        
                        <loadtasks assembly=""${nant::get-base-directory()}\Tools.MigrationHelper.Core.dll"" />
                        <registerindi connectionstring =""" + connectionString + @"""></registerindi> 
                        <" + action + @" connectionstring =""" + connectionString + @""" />
                    </project>";


            return RunBuild(xml).Project;

        }


		/// <summary>
		/// Выполняет sql-команду и возвращает скаларное значение указанного типа
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="command"></param>
		/// <returns></returns>
		private T ExecuteScalar<T>(string command)
		{
			T result = default(T);

			try
			{
				_connection.Open();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

			try
			{
				SqlCommand myCommand = new SqlCommand(command, this._connection);
				result = (T)myCommand.ExecuteScalar();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

			_connection.Close();
			return result;
		}

		/// <summary>
		/// Удаление файлов xml из папок проета DbStructure и DbEnums
		/// </summary>
		/// <param name="directoryInfo"></param>
		public void RevertFiles(DirectoryInfo directoryInfo)
		{
			if (directoryInfo == null)
				return;
			if (!directoryInfo.FullName.Contains("DbStructure") && !directoryInfo.FullName.Contains("DbEnums"))
			{
				DirectoryInfo directoryStr = directoryInfo.GetDirectories("DbStructure").FirstOrDefault();
				RevertFiles(directoryStr);
				DirectoryInfo directoryEnm = directoryInfo.GetDirectories("DbEnums").FirstOrDefault();
				RevertFiles(directoryEnm);
			}
			else
			{
				using (var svnClient = new SvnClient())
				{
					svnClient.Revert(directoryInfo.FullName, new SvnRevertArgs { Depth = SvnDepth.Infinity });
				}
			}
		}
		
		#endregion

	}
}
