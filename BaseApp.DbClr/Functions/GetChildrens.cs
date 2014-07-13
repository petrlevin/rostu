using System;
using System.Collections;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Functions
{
	/// <summary>
	/// Класс реализующий получение идентификаторов детей для указанного элемента при построении иерархии по указанному полю
	/// </summary>
	public class GetChildrens
	{
		/// <summary>
		/// Получение идентификаторов детей для указанного элемента
		/// </summary>
		/// <param name="idItem">Элемент, для которого получаются дети</param>
		/// <param name="idParentField">Поле по которому строится иерархия</param>
		/// <param name="includeThis">Включать в результат зам элемент да/нет</param>
		/// <returns></returns>
		[SqlFunction(DataAccess = DataAccessKind.Read, FillRowMethodName = "FillRow", TableDefinition="id int")]
		public static IEnumerable Get(int idItem, int idParentField, bool includeThis)
		{
			EntityField parentEntityField;
			try
			{
				parentEntityField = Objects.ById<EntityField>(idParentField);
			} catch
			{
				SqlContext.Pipe.Send("Ошибка при получении поля сущности по идентифкатору '"+idParentField+"'");
				throw;
			}
			Entity entity = parentEntityField.Entity;
			ArrayList result = new ArrayList();
			using (SqlConnection connection=new SqlConnection("context connection=true"))
			{
				connection.Open();
				string textCommand = string.Format(
					"WITH cte AS (SELECT [id], [{3}] FROM [{0}].[{1}] WHERE [{4}]={2} UNION ALL SELECT [b].[id], [b].[{3}] FROM [cte] a INNER JOIN [{0}].[{1}] b ON [b].[{3}]=[a].[id]) SELECT [id] FROM [cte]",
					entity.Schema, entity.Name, idItem, parentEntityField.Name,
					includeThis ? "id" : parentEntityField.Name);

				using (SqlCommand command = new SqlCommand(textCommand, connection))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(new Result(reader.GetInt32(0)));
						}
					}
				}
				connection.Close();
			}
			return result;
		}

		/// <summary>
		/// Класс описывающий строку результирующего набора
		/// </summary>
		private class Result
		{
			public SqlInt32 Id;

			public Result(SqlInt32 id)
			{
				Id = id;
			}
		}

		/// <summary>
		/// Заполнение строки результата
		/// </summary>
		/// <param name="value">Значение в виде object</param>
		/// <param name="id">Значение в виде SqlInt32</param>
		public static void FillRow(object value, out SqlInt32 id)
		{
			Result result = (Result)value;
			id = result.Id;
		}
	}
}
