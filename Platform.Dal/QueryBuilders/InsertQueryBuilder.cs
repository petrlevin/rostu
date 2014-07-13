using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Collections;

namespace Platform.Dal.QueryBuilders
{
	/// <summary>
	/// Класс для построение выражения INSERT
	/// </summary>
	public class InsertQueryBuilder : QueryBuilder, IInsertQueryBuilder
	{
		#region Private Fields

		/// <summary>
		/// Cловарь "Имя поля", "Значение"
		/// </summary>
		private readonly Dictionary<String,object> _fieldsWithValue;

		/// <summary>
		/// Наименование полей, которые получат значения при вставке
		/// </summary>
		private readonly List<string> _fieldNames;

		#endregion
		
		#region Constructors

		/// <summary>
		/// Конструктор для построения выражения INSERT
		/// </summary>
		/// <param name="entity">Форма</param>
		/// <param name="fieldsWithValue">Поля и их значения</param>
        public InsertQueryBuilder(IEntity entity, Dictionary<String,object> fieldsWithValue)
            : base(entity)
		{
			_fieldNames = fieldsWithValue.Select(a => a.Key).ToList();
			_fieldsWithValue = fieldsWithValue;
		}

		/// <summary>
		/// Конструктор для построения выражения INSERT
		/// </summary>
		/// <param name="form">Форма</param>
		/// <param name="fieldNames">Наименование полей</param>
        public InsertQueryBuilder(IEntity entity, List<string> fieldNames)
            : base(entity)
		{

			_fieldNames = fieldNames;
		}
	
		/// <summary>
		/// Конструктор без параметров
		/// </summary>
		public InsertQueryBuilder() : base()
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
				throw new Exception("Entity is null");

			List<string> fieldNames;
			if (_fieldNames == null || _fieldNames.Count == 0)
				fieldNames = Entity.RealFields
					.Where(f => !f.IdCalculatedFieldType.HasValue)
					.Select(f => f.Name)
					.ToList();
			else
				fieldNames = Entity.RealFields
					.Where(f => !f.IdCalculatedFieldType.HasValue && _fieldNames.Contains(f.Name, StringComparer.OrdinalIgnoreCase))
					.Select(f => f.Name)
					.ToList();
			InsertStatement result = new Insert(fieldNames, Entity.Schema, Entity.Name).GetQuery();

			if (_fieldsWithValue != null && _fieldsWithValue.Count > 0)
				result.SourceAsValues(_fieldsWithValue);
			return result;
		}

		/// <summary>
		/// Инициализирует список декораотров, необходимых для построителя запросов на вставку.
		/// </summary>
		protected override void InitPrivateDecorators()
		{
			BeforePrivateDecorators.Add(new AddIdentityToOutput());
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

		#region Implementation of IInsertQueryBuilder

		/// <summary>
		/// Возвращать значение идентификатора. После команды INSERT будет выполнена команда SELECT SCOPE_IDENTITY().
		/// </summary>
		public bool ReturnIdentityValue { get; set; }
		#endregion

		private const string _forAddOutput = "declare @tmp table (id int);{0};select id from @tmp;";

		public override SqlCommand GetSqlCommand(SqlConnection connection)
		{
			SqlCommand result = base.GetSqlCommand(connection);
			if (result.CommandText.Contains("OUTPUT") && result.CommandText.Contains("INTO "))
			{
				result.CommandText = string.Format(_forAddOutput, result.CommandText);
			}
			return result;
		}
	}
}
