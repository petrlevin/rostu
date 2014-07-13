using System;
using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Dal.Decorators;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using System.Linq;

namespace Platform.Dal.QueryBuilders
{
	/// <summary>
	/// Построитель запросов на выборку (SELECT)
	/// </summary>
	public class SelectQueryBuilder : QueryBuilder, ISelectQueryBuilder
	{
		#region Constructors

		/// <summary>
		/// Построитель запросов на выборку (SELECT)
		/// </summary>
		/// <param name="form">Форма</param>
		/// <param name="fieldNames">Наименование отбираемых полей</param>
        public SelectQueryBuilder(IEntity entity, List<string> fieldNames)
            : base(entity)
		{

			Fields = fieldNames;
			AliasName = "a"; // Алиас по-умолчанию.
		}

		/// <summary>
		/// Построитель запросов на выборку (SELECT)
		/// </summary>
		/// <param name="form">Форма</param>
        public SelectQueryBuilder(IEntity entity)
            : this(entity, null)
		{
		}

		/// <summary>
		/// Построитель запросов на выборку (SELECT)
		/// </summary>
		public SelectQueryBuilder() : this(null, null)
		{
		}

		#endregion

		#region Implementation of IDeleteQuery

		/// <summary>
		/// Условия отбора. Как для выборки, так и для удаления. 
		/// При Вставке (Insert) игнорируется.
		/// </summary>
		public IFilterConditions Conditions { get; set; }

		#endregion

		#region Implementation of ISelectQuery

		/// <summary>
		/// Алиас для тела запроса
		/// </summary>
		public string AliasName { get; set; }

		/// <summary>
		/// Список полей для отбора. Данные поля будут включены в выборку.
		/// Если список пуст, то будут отобраны все поля.
		/// </summary>
		/// <remarks>
		/// - List&lt;string&gt; вместо List&lt;EntityField&gt; используется для возможности (де)сериализации.
		/// - Точка данных тоже имеет поля. Некоторые поля могут быть вычисляемыми и здесь перечисляются только те, которые следует включить в выборку.
		/// </remarks>
		public virtual List<string> Fields { get; set; }

		/// <summary>
		/// Пэйджинг - номер первой записи в выборке и количество записей.
		/// </summary>
		public IPaging Paging { get; set; }

		/// <summary>
		/// Информация о сортировке
		/// </summary>
		public IOrder Order { get; set; }

		/// <summary>
		/// Строка поиска в гриде
		/// </summary>
		public string Search { get; set; }

		#endregion

		#region Overrides of Query

		/// <summary>
		/// Возвращает объектную модель запроса, готовую для обработки декораторами и валидаторами.
		/// Чтобы получить тект запроса, вызовите метод Render() у возвращенного объекта.
		/// </summary>
		/// <returns>TSqlStatement</returns>
		public override TSqlStatement GetTSqlStatement()
		{
			if (Entity== null)
				throw new Exception("SelectQuery: передан пустой TargetFieldsSet");

			List<string> fieldNames;
			List<string> realFields = Entity.RealFields.Select(f => f.Name).ToList();

			if (Fields == null || Fields.Count == 0)
				fieldNames = realFields;
			else
				fieldNames = Fields.Where(realFields.Contains).ToList();

            /* Не будем выдавать исключение. Вместо этого просто отбросим все нехранимые в таблице поля.
             * Т.к. некоторые из них могут быть обработаны декораторами. 
             * Например поля типа "Мультиссылка" будут обработаны декоратором MultilinkAsString
            IEnumerable<string> wrongFields = fieldNames.Where(a => !realFields.Contains(a, StringComparer.OrdinalIgnoreCase));
            if (wrongFields.Any())
                throw new Exception(string.Format(
                    "SelectQuery: в сущности {0} отсутствуют перечисленные поля: {1}",
                    Entity.Name,
                    string.Join(", ", wrongFields)));
            */

			SelectStatement result = new Select(fieldNames, Entity.Schema, Entity.Name, AliasName).GetQuery();
			return result;
		}

		/// <summary>
		/// Инициализирует список декораторов, необходимых для построителя запросов на выборку.
		/// </summary>
		protected override void InitPrivateDecorators()
		{
			MultilinkAsString multilinks = new MultilinkAsString();
            AddCaptions captions = new AddCaptions();
            AddDescriptions descriptions = new AddDescriptions();
			AddJoinedFields joinedFields = new AddJoinedFields();
			AddWhere condidions = new AddWhere();
			AddOrder order = new AddOrder();
			AddPaging paging = new AddPaging();
			AddDbComputedFunctionFields dbComputed = new AddDbComputedFunctionFields();
			//AddGridSearch gridSearch = new AddGridSearch();

			BeforePrivateDecorators.AddRange(new List<TSqlStatementDecorator>
				{
					condidions,
				});
			AfterPrivateDecorators.AddRange(new List<TSqlStatementDecorator>
				{
					multilinks,
					dbComputed,
                    captions,
                    descriptions,
					joinedFields,  // пока присоединенные поля не могут быть ссылочными данный декоратор не обязательно должен следовать до addCaptions и addDescriptions
					//gridSearch,
					order,
                    paging,
				});
		}

		#endregion
	}
}
