using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.SqlObjectModel;

namespace Platform.Dal.QueryBuilders
{
	public class ReqursiveSelectQueryBuilder : QueryBuilder, ISelectQueryBuilder
	{
		private readonly string _namePartWith;
		private readonly string _reqursiveFieldName;
		private readonly string _parentReqursiveFieldName;

		public ReqursiveSelectQueryBuilder(string namePartWith, string reqursiveFieldName, string parentReqursiveFieldName)
		{
			_namePartWith = namePartWith;
			_reqursiveFieldName = reqursiveFieldName;
			_parentReqursiveFieldName = parentReqursiveFieldName;
			AliasName = "a";
		}
		
		#region Overrides of QueryBuilder

		/// <summary>
		/// Метод возвращающий выражение в виде объектной модели из пространства Microsoft.Data.Schema.ScriptDom.Sql
		/// Чтобы получить тект запроса, вызовите метод Render() у возвращенного объекта.
		/// </summary>
		/// <returns></returns>
		public override TSqlStatement GetTSqlStatement()
		{
			if (Entity == null)
				throw new Exception("SelectQuery: передан пустой TargetFieldsSet");

			List<string> fieldNames;
			List<string> realFields = Entity.RealFields.Select(f => f.Name).ToList();
			if (Fields == null || Fields.Count == 0)
				fieldNames = realFields;
			else
				fieldNames = Fields;

			if (fieldNames.Any(a => !realFields.Select(real => real).Contains(a, StringComparer.OrdinalIgnoreCase)))
				throw new Exception("Перечисленные поля отсутствуют в сущности '" + Entity.Name + "' - " + string.Join(",", fieldNames.Where(a => !realFields.Contains(a))));

			RecursiveSelect select = new RecursiveSelect(fieldNames, Entity.Schema, Entity.Name, _namePartWith, AliasName, _reqursiveFieldName, _parentReqursiveFieldName);
			return select.GetQuery();
		}

		protected override void InitPrivateDecorators()
		{
			return;
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
	}
}
