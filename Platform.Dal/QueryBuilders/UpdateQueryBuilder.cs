using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.QueryBuilders
{
	/// <summary>
	/// Класс для построение выражения UPDATE
	/// </summary>
	public class UpdateQueryBuilder : QueryBuilder, IUpdateQueryBuilder
	{
		#region Private Fields

		/// <summary>
		/// Cловарь "Имя поля", "Значение"
		/// </summary>
		private readonly Dictionary<string, object> _fieldsWithValue;

		#endregion

		#region Constructors

		/// <summary>
		/// Конструктор для построения выражения UPDATE
		/// </summary>
        /// <param name="entity">Форма</param>
		/// <param name="fieldsWithValue">Поля и их значения</param>
		public UpdateQueryBuilder(IEntity entity, Dictionary<string, object> fieldsWithValue):base(entity)
		{
			List<string> validFieldName =
				entity.RealFields.Where(f => !f.IdCalculatedFieldType.HasValue).Select(
					f => f.Name).ToList();
			_fieldsWithValue = fieldsWithValue.Where(a=> validFieldName.Contains(a.Key, StringComparer.OrdinalIgnoreCase)).ToDictionary(a=> a.Key, b=> b.Value);
		}

		/// <summary>
		/// Констурктор
		/// </summary>
        /// <param name="entity">Сущность</param>
		public UpdateQueryBuilder(IEntity entity) : this(entity, null)
		{
		}

		/// <summary>
		/// Конструктор без параметров
		/// </summary>
		public UpdateQueryBuilder() : this(null)
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
			if (Entity == null)
				throw new Exception("У переданого Form не заполнено свойство Entity");

			UpdateStatement result = new Update(Entity.Schema, Entity.Name, "a").GetQuery();

			if (_fieldsWithValue != null && _fieldsWithValue.Count > 0)
				result.SetAsValues(_fieldsWithValue);

			return result;
		}

		/// <summary>
		/// Инициализирует список декораотров, необходимых для построителя запросов на обновление.
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

		#region Implementation of IUpdateQuery

		/// <summary>
		/// Поля и значения.
		/// </summary>
		public Dictionary<string, string> FieldValues { get; set; }

		#endregion

	}
}
