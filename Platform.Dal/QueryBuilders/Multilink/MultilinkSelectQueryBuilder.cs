using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Dal.Decorators;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Select = Platform.SqlObjectModel.Select;

namespace Platform.Dal.QueryBuilders.Multilink
{
	/// <summary>
	/// Построитель запросов на выборку (SELECT) для сущности с типом мультилинк
	/// </summary>
	public class MultilinkSelectQueryBuilder : QueryBuilder, ISelectQueryBuilder
	{
		private IEntityField _leftMultilinkField;
		private IEntityField _rightMultilinkField;

		/// <summary>
        /// Левое поле (по отношению к сущности <paramref name="MultilinkOwnerId"/>) мультиссылки
		/// </summary>
		private IEntityField LeftMultilinkField
		{
			get
			{
				if (_leftMultilinkField == null)
					_leftMultilinkField = EntityMl.Fields.Single(f => f.IdEntityLink == MultilinkOwnerId);
				return _leftMultilinkField;
			}
		}

		/// <summary>
        /// Правое поле (по отношению к сущности <paramref name="MultilinkOwnerId"/>) мультиссылки
		/// </summary>
		private IEntityField RightMultilinkField
		{
			get
			{
				if (_rightMultilinkField == null)
					_rightMultilinkField = EntityMl.Fields.Single(f => f.EntityFieldType == EntityFieldType.Link && f.IdEntityLink != MultilinkOwnerId);
				return _rightMultilinkField;
			}
		}

		/// <summary>
		/// Идентификатор сущности, содержащей мультиссылку
		/// </summary>
		public int MultilinkOwnerId { get; set; }

		/// <summary>
		/// Значение идентификатора элемента сущности, содержащего мультиссылку
		/// </summary>
		public int FilterValue;

		/// <summary>
		/// Сущность мультилинка
		/// </summary>
		public IEntity EntityMl;

		/// <summary>
		/// Инициализирует новый экземпляр класса MultilinkSelectQueryBuilder
		/// </summary>
		public MultilinkSelectQueryBuilder() : this(null, null, null, 0, 0)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса MultilinkSelectQueryBuilder
		/// </summary>
		/// <param name="form">Форма</param>
		/// <param name="fieldNames">Набор полей для выборки</param>
		/*public MultilinkSelectQueryBuilder(IEntity entity, List<string> fieldNames)
            : this(entity, fieldNames, 0, 0)
		{
		}*/

		/// <summary>
		/// Инициализирует новый экземпляр класса MultilinkSelectQueryBuilder
		/// </summary>
		/// <param name="form">Форма</param>
		/// <param name="multilinkOwnerId">Поле мультилинка по которому осуществляется фильтрация</param>
        /*public MultilinkSelectQueryBuilder(IEntity entity, int multilinkOwnerId, int filterValue)
			: this(entity, null, multilinkOwnerId, filterValue)
		{
		}*/

		/// <summary>
		/// Инициализирует новый экземпляр класса MultilinkSelectQueryBuilder
		/// </summary>
		/// <param name="form">Форма</param>
		/// <param name="fieldNames">Набор полей для выборки</param>
		/// <param name="multilinkOwnerId">
		/// Идентификатор сущности, содержащей мультиссылку. 
		/// По этой информации будет вычислено имя поля мультиссылки, по которому будет осуществляется фильтрация.
		/// </param>
        public MultilinkSelectQueryBuilder(IEntity entity, IEntity entityMl, List<string> fieldNames, int multilinkOwnerId, int filterValue)
            : base(entity)
		{
			Fields = fieldNames;
			MultilinkOwnerId = multilinkOwnerId;
			FilterValue = filterValue;
			AliasName = "a";
			EntityMl = entityMl;
		}


		#region Overrides of QueryBuilder

		/// <summary>
		/// Метод возвращающий выражение в виде объектной модели из пространства Microsoft.Data.Schema.ScriptDom.Sql
		/// Чтобы получить тект запроса, вызовите метод Render() у возворащенного объекта.
		/// </summary>
		/// <returns></returns>
		public override TSqlStatement GetTSqlStatement()
		{
			if (Entity == null || EntityMl == null)
				throw new Exception("Передан пустой TargetFieldsSet");
			if (MultilinkOwnerId == 0)
				throw new Exception("Не задан MultilinkOwnerId");
			if (RightMultilinkField.IdEntityLink == null)
				throw new Exception("У правого поля мультиссылки не задано свойство IdEntityLink");

			Fields = (Fields == null || Fields.Count == 0) ? Entity.RealFields.Select(a => a.Name).ToList() : Fields;

			var result = CreateSelectStatement(Entity);
			BinaryExpression first = Helper.CreateBinaryExpression("a.id".ToColumn(), ("ml." + RightMultilinkField.Name).ToColumn(),
			                                          BinaryExpressionType.Equals);
			BinaryExpression second = Helper.CreateBinaryExpression(("ml." + LeftMultilinkField.Name).ToColumn(), FilterValue.ToLiteral(),
			                                           BinaryExpressionType.Equals);

			result.Join(QualifiedJoinType.Inner,
			            Helper.CreateSchemaObjectTableSource(EntityMl.Schema, EntityMl.Name, "ml"),
			            Helper.CreateBinaryExpression(first, second, BinaryExpressionType.And));

			return result;
		}

	    protected virtual SelectStatement CreateSelectStatement(IEntity entity)
	    {
	        SelectStatement result = new Select(Fields, entity.Schema, entity.Name, AliasName).GetQuery();
	        return result;
	    }

	    protected override void InitPrivateDecorators()
		{
			AddCaptions captions = new AddCaptions();
            AddDescriptions descriptions = new AddDescriptions();
			AddJoinedFields joinedFields = new AddJoinedFields();
			AddWhere condidions = new AddWhere();
			AddOrder order = new AddOrder();
			AddPaging paging = new AddPaging();
			//AddGridSearch gridSearch = new AddGridSearch();

			BeforePrivateDecorators.AddRange(new List<TSqlStatementDecorator>
				{
					condidions,
				});

			AfterPrivateDecorators.AddRange(new List<TSqlStatementDecorator>
				{
					captions,
                    descriptions,
					joinedFields,
					//gridSearch,
					order,
					paging,
				});

		}

		#endregion

		#region Implementation of IDeleteQueryBuilder

		/// <summary>
		/// Условия отбора. Как для выборки, так и для удаления. 
		/// При Вставке (Insert) игнорируется.
		/// </summary>
		public IFilterConditions Conditions { get; set; }

		#endregion

		#region Implementation of ISelectQueryBuilder

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
		public List<string> Fields { get; set; }

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

		/// <summary>
		/// По сущности, содержащей мультиссылку, определяет имя поля мультиссылки, по которому следует фильтровать
		/// </summary>
		/// <returns></returns>
		private string getFilterFieldName()
		{
			if (MultilinkOwnerId < 1)
				return string.Empty;

			return Entity.Fields.Single(f => f.IdEntityLink == MultilinkOwnerId).Name;
		}
	}
}
