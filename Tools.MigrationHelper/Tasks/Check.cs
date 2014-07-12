using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Tools.MigrationHelper.Tasks.CheckTask;

namespace Tools.MigrationHelper.Tasks
{
	/// <summary>
	/// Проверка консистентности БД
	/// </summary>
	class Check : MhTaskBase
	{
		private SqlConnection connection;

		/// <summary>
		/// Запуск тестов
		/// </summary>
		/// <param name="config"></param>
		/// <param name="output"></param>
		public override int Process(MhConfiguration config, System.IO.TextWriter output)
		{
			base.Process(config, output);
			_output.WriteLine("Проверка консистентности БД");
			connection = new SqlConnection(_config.ConnectionString);
			connection.Open();

			int failCnt = 0;
			failCnt += checkInclassTests();
			failCnt += checkDeclarativeTests();

			connection.Close();

			return failCnt;
		}

		/// <summary>
		/// Проверка тестов, описанных внутри данного класса при помощи атрибута <see cref="TestAttribute"/>.
		/// </summary>
		private int checkInclassTests()
		{
			int failCnt = 0;
			IEnumerable<MethodInfo> methodInfos = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
			foreach (var methodInfo in methodInfos)
			{
				TestAttribute attr = methodInfo.GetCustomAttribute<TestAttribute>();
				if (attr != null && !attr.Ignore)
				{
					var d = methodInfo.CreateDelegate(typeof (Func<bool>), this);
					var result = (bool)d.DynamicInvoke();
					failCnt += result ? 0 : 1;
				}
			}
			return failCnt;
		}

		/// <summary>
		/// Проверка тестов, описанных декларативно в xml файле. <seealso cref="CheckTaskResource"/>
		/// </summary>
		private int checkDeclarativeTests()
		{
			int failCnt = 0;

			foreach (var test in getDeclarativeTests())
			{
				test.Connection = this.connection;

				bool result = test.Check();
				if (!result)
				{
					_output.WriteLine("Failed: {0}{1}{2}", test.Title, Environment.NewLine, test.SqlCommand);
					failCnt++;
				}
			}
			return failCnt;
		}

		/// <summary>
		/// Получение списка декларативных тестов.
		/// </summary>
		/// <remarks>
		/// Сначала происходит попытка получить список тестов напрямую из xml файла, в свойствах которого указано Copy to output directory = copy always.
		/// Если данный файл не найден, происходит попытка прочитать его из присоединенных ресурсов.
		/// Чтение из xml напрямую позволяет менять этот файл без перекомпиляции.
		/// Использование ресурса помогает, когда код проверок вызван из теста TeamCity, или при отладке теста. В этом случае в директории с исполняемым файлом xml отсутствует, 
		/// но есть возможность получить его через присоединенный ресурс.
		/// </remarks>
		/// <returns></returns>
		private List<Test> getDeclarativeTests()
		{
			string name = "Tests";
			string fileName;
			TextReader reader;
			var serializer = new XmlSerializer(typeof(List<Test>));

			fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + string.Format(@"\Tasks\CheckTask\{0}.xml", name);

			if (File.Exists(fileName))
			{
				reader = File.OpenText(fileName);
			}
			else
			{
				var xml = CheckTaskResource.ResourceManager.GetString(name);
				reader = new StringReader(xml);
			}

			return (List<Test>)serializer.Deserialize(reader);
		}

		private SqlDataReader ExecuteReader(string sql)
		{
			SqlCommand cmd = new SqlCommand(sql, connection);
			return cmd.ExecuteReader();
		}

		private T ExecuteScalar<T>(string sql)
		{
			SqlCommand cmd = new SqlCommand(sql, connection);
			return (T)cmd.ExecuteScalar();
		}

		#region Тесты, описанные внутри класса через атрибут TestAttribute
		
		/// <summary>
		/// ВНИМАНИЕ! Данный тест здесь лишь для примера. Он описан в декларативном виде, в Tests.xml. <seealso cref="CheckTaskResource"/>.
		/// </summary>
		[Test(Ignore = true)]
		public bool idOwner_MustHave_idEntityLink()
		{
			var sql = "SELECT COUNT(*) FROM [ref].[EntityField] WHERE NAME = 'idOwner' AND idEntityLink IS NULL";
			int result = ExecuteScalar<int>(sql);

			if (result > 0)
			{
				_output.WriteLine(string.Format("Failed: Следующий запрос вернул записи: {0}", sql));
				return false;
			}
			return true;
		}


		#endregion
	}
}
