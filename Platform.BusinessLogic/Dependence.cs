using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.BusinessLogic
{
	/// <summary>
	/// Вычисление зависимых элементов
	/// </summary>
	public class Dependence
	{
		private int _id;

		private int _idEntity;

		private Entity _entity;

		/// <summary>
		/// Соединение с БД
		/// </summary>
		private static readonly SqlConnection _connection = IoC.Resolve<SqlConnection>("DbConnection");

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="id">Идентификатор элемента</param>
		/// <param name="idEntity">Идентификатор сущности</param>
		public Dependence(int id, int idEntity)
		{
			_id = id;
			_idEntity = idEntity;
			_entity = Objects.ById<Entity>(_idEntity);
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="id">Идентификатор элемента</param>
		/// <param name="entity">Сущность</param>
		public Dependence(int id, Entity entity)
		{
			_id = id;
			_idEntity = entity.Id;
			_entity = entity;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>идентифкатор сущности, список элементов</returns>
		public Dictionary<int, List<int>> GetResult()
		{
			Platform.SqlObjectModel.Select select = new SqlObjectModel.Select("Name, idEntity", "ref", "EntityField", "a");
			SelectStatement queryEntityField = select.GetQuery();
			queryEntityField = queryEntityField.Where(BinaryExpressionType.And,
			                Helper.CreateBinaryExpression("a.idEntityLink".ToColumn(), _idEntity.ToLiteral(),
			                                              BinaryExpressionType.Equals));
			OrderByClause order=new OrderByClause();
			order.OrderByElements.Add(new ExpressionWithSortOrder {Expression = "a.identity".ToColumn()});
			queryEntityField.OrderByClause = order;
			using (SqlCommand command = new SqlCommand(queryEntityField.Render(), _connection))
			{
				try
				{
					_connection.Open();
				}
				finally
				{
					_connection.Close();
				}
			}
			
			/*
			 1. получить поля, которые ссылаются на указанную сущность и идентификаторы сущности к которой они принадлежат
			 2. получить строки из сущностей п.1 где выбранные поля из п.1 ссылаются на указанный элемент
			 2.1. общие ссылки
			 3. если строка является ТЧ то опеределить родителя и его наименование
			 4. как то все это сгруппировать и возможно отсортировать
			*/
			return null;
		}
	}
}
