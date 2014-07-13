using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.Interfaces;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;
using Platform.SqlObjectModel.Extensions;

namespace Platform.BusinessLogic.Decorators
{
	/// <summary>
	/// Класс описывающий реализацию декоратора для формы выбора Сущности у общей ссылки
	/// </summary>
	public class AddFilterEntitiesForGenericLink : TSqlStatementDecorator, IOrdered
	{
		#region Private fields

		private int? _entityFieldId;
		/// <summary>
		/// Построитель запроса
		/// </summary>
		private ISelectQueryBuilder _builder;

		private EntityFieldType[] _generigLinks = new[]
			{
				EntityFieldType.ReferenceEntity,
				EntityFieldType.DocumentEntity,
				EntityFieldType.TablepartEntity,
				EntityFieldType.ToolEntity
			};
		#endregion

		#region Constructors
		/// <summary>
		/// Дефолтный конструктор закрыт, без параметров все равно не отработает
		/// </summary>
		private AddFilterEntitiesForGenericLink()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="EntityFieldId"></param>
		public AddFilterEntitiesForGenericLink(int EntityFieldId)
		{
			_entityFieldId = EntityFieldId;
		}
		#endregion
		#region Implementation of ITSqlStatementDecorator
		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			if (!_entityFieldId.HasValue)
				return source;
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null)
				return source;
			if (!_builder.Entity.Name.Equals("entity", StringComparison.OrdinalIgnoreCase))
				return source;
			Entity entity = (_builder.Entity as Entity);
			EntityField entityField = Objects.ById<EntityField>(_entityFieldId.Value);
			if (!entityField.Entity.Fields.Any(a => a.Id == _entityFieldId && a.Name.EndsWith("entity", StringComparison.OrdinalIgnoreCase) && entityField.Entity.Fields.Any(b => _generigLinks.Contains(b.EntityFieldType) && b.Name.Equals(a.Name.Substring(0, a.Name.Length - 6)))))
				return source;

			SelectStatement result = (source as SelectStatement);
			if (result == null)
				return source;

			result = result.Where(BinaryExpressionType.And,
			                      Helper.CreateBinaryExpression((_builder.AliasName + ".AllowGenericLinks").ToColumn(),
			                                                    1.ToLiteral(), BinaryExpressionType.Equals));
			//this.Log(result, queryBuilder);
			return result;
		}
		#endregion

		#region Implementation of IOrdered
		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> Before { get; private set; }

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After
		{
			get
			{
				return new List<Type>()
					{
						typeof(AddWhere)
					};
			}
		}

		/// <summary>
		/// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
		/// </summary>
		public Order WantBe { get; private set; }
		#endregion
	}
}
