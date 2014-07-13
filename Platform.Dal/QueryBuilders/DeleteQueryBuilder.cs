using System;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;

namespace Platform.Dal.QueryBuilders
{
	/// <summary>
	/// Класс для построение выражения DELETE
	/// </summary>
	public class DeleteQueryBuilder : QueryBuilder, IDeleteQueryBuilder
	{
		#region Constructors

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="entity">Сущность</param>
		public DeleteQueryBuilder(IEntity entity):base(entity)
		{

		}

		/// <summary>
		/// Конструктор без параметров
		/// </summary>
		public DeleteQueryBuilder() : this (null)
		{
		}
		
		#endregion

		#region Overrides of Query

		/// <summary>
		/// Возвращает объектную модель запроса, готовую для обработки декораторами и валидаторами.
		/// Чтобы получить тект запроса, вызовите метод Render() у возвращенного объекта.
		/// </summary>
		/// <returns></returns>
		public override TSqlStatement GetTSqlStatement()
		{
			if (Entity==null)
				throw new Exception("У переданого Form не заполнено свойство Entity");
			DeleteStatement result = new Delete(Entity.Schema, Entity.Name, "a").GetQuery();
			return result;
		}

		/// <summary>
		/// Инициализирует список декораотров, необходимых для построителя запросов на удаление.
		/// </summary>
		protected override void InitPrivateDecorators()
		{
			this.BeforePrivateDecorators.Add(new AddWhere());
		}

		#endregion

		#region Implementation of IDeleteQuery

		/// <summary>
		/// Условия отбора. Как для выборки, так и для удаления. 
		/// При Вставке (Insert) игнорируется.
		/// </summary>
		public IFilterConditions Conditions { get; set; }

		#endregion


	}
}
