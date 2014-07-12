using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.Tasks.CheckTask;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("check")]
    public class Check: MhTask 
    {
        private SqlConnection _connection;

        [TaskAttribute("connectionstring", Required = true)]
        public string ConnectionString
        {
            get;
            set;
        }


        protected override void ExecuteTask()
        {
			Log(Level.Info, "Проверка консистентности БД");
            SetResult(true);
            using (_connection = new SqlConnection(ConnectionString))
            {
                Log(Level.Verbose, "Подключение к базе данных");
                _connection.Open();


                CheckInclassTests();
                CheckDeclarativeTests();

            }

            Log(Level.Info, _success ? "БД консистентна" : "БД не консистентна");
        }

        public Check()
        {
            FailOnError = false;
        }


        private bool _success;
        private void SetResult(bool success)
        {
            if (!Project.Properties.Contains("check.result"))
                Project.Properties.Add("check.result", success.ToString().ToLower());
            else
                Project.Properties["check.result"] = success.ToString().ToLower();
            _success = success;
        }


        protected override void ErrorFormat(string format, params object[] args)
        {
            base.ErrorFormat(format, args);
            SetResult(false);
        }

		/// <summary>
		/// Проверка тестов, описанных внутри данного класса при помощи атрибута <see cref="Tools.MigrationHelper.Core.Tasks.CheckTask.TestAttribute"/>.
		/// </summary>
		private void CheckInclassTests()
		{

			IEnumerable<MethodInfo> methodInfos = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
			foreach (var methodInfo in methodInfos)
			{
				var attr = methodInfo.GetCustomAttribute<TestAttribute>();
				if (attr != null && !attr.Ignore)
				{
				    try
				    {
                        try
                        {
                            Log(Level.Verbose, "Выполение inclass теста - {0}", methodInfo.Name);
                            var d = methodInfo.CreateDelegate(typeof(Func<bool>), this);
                            var result = (bool)d.DynamicInvoke();
                            if (result)
                                Log(Level.Verbose, "Sucsess");


                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException;
                        }

				    }
				    catch (BuildException)
				    {
				        throw;
				    }
				    catch (Exception ex )
				    {
                        FatalFormat("Фатальная ошибка при выполнении inclass тестов. Имя метода теста - '{0}'." ,ex,methodInfo.Name);
				        //throw;
				    }
					
				}
			}

		}


        private bool CheckTest(Test test)
        {
            try
            {
                return test.Check();
            }
            catch (Exception ex)
            {
                return FatalFormat<bool>("Фатальная ошибка при выполнении декларативного теста {0}",ex,test.Title);
            }
        }

		/// <summary>
		/// Проверка тестов, описанных декларативно в xml файле. <seealso cref="Tools.MigrationHelper.Core.Tasks.CheckTask.CheckTaskResource"/>
		/// </summary>
		private int CheckDeclarativeTests()
		{
			int failCnt = 0;

			foreach (var test in GetDeclarativeTests())
			{
				test.Connection = this._connection;
                Log(Level.Verbose, "Выполение декларативного  теста - {0}", test.Title);
			    bool result = CheckTest(test);
				
				if (!result)
				{
                    ErrorFormat("Failed: {0}{1}{2}", test.Title, Environment.NewLine, test.SqlCommand);
					failCnt++;
				}
				else
				{
                    Log(Level.Verbose, "Sucsess");    
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
		private List<Test> GetDeclarativeTests()
		{
		    try
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
		    catch (Exception ex)
		    {
                return FatalFormat<List<Test>>("Фатальная ошибка при получении декларативных тестов", ex);

		    }
		}


		private T ExecuteScalar<T>(string sql)
		{
			SqlCommand cmd = new SqlCommand(sql, _connection);
			return (T)cmd.ExecuteScalar();
		}

		#region Тесты, описанные внутри класса через атрибут TestAttribute
		
		/// <summary>
		/// ВНИМАНИЕ! Данный тест здесь лишь для примера. Он описан в декларативном виде, в Tests.xml. <seealso cref="Tools.MigrationHelper.Core.Tasks.CheckTask.CheckTaskResource"/>.
		/// </summary>
		[Test(Ignore = true)]
		public bool idOwner_MustHave_idEntityLink()
		{
			var sql = "SELECT COUNT(*) FROM [ref].[EntityField] WHERE NAME = 'idOwner' AND idEntityLink IS NULL";
			int result = ExecuteScalar<int>(sql);
		    
			if (result > 0)
			{
				ErrorFormat("Failed: Следующий запрос вернул записи: {0}", sql);
				return false;
			}
			return true;
		}

        [Test(Ignore = true)]
        public bool GreatWoolfTest()
        {
            ErrorFormat("Failed: GreatWoolfTest: {0}", "RUDOLF");
            return false;
        }

        [Test(Ignore = true)]
        public bool GreatDogfTest()
        {
            //throw new Exception("бла бла бла",new NotImplementedException("AAAAAAAAAAAAAA"));
            ErrorFormat("Failed: GreatDogfTest: {0}", "RUDOLF");
            return false;
        }




		#endregion
	}
    
}
