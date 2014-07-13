using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Rights
{

    /// <summary>
    /// При получении данных для формы выбора элемента ТЧ, ссылающегося на вышестоящий элемент (idPerent)
    /// данный декоратор добавляет фильтр на поле idOwner. 
    /// Т.о. форма выбора отобразить только элементы, принадлежащие тому же вышестоящему элементу.
    /// </summary>
    public class AddFilterOnTablepartOwnerFieldForParentLink : TSqlStatementDecorator, IApplyForAggregate
	{
		private ISelectQueryBuilder _builder;

		private IEntity _entity;

		private int _idOwnerValue;

		/// <summary>
		/// Идентификатор поля для которого создается фильтр
		/// </summary>
		private readonly int _idEntityField;

		/// <summary>
		/// Дефолтный конструктор закрываем, чтобы не делать поле _idEntityField public
		/// </summary>
		private AddFilterOnTablepartOwnerFieldForParentLink()
		{
		}

        /// <summary>
        /// Создаем декоратор
        /// </summary>
        /// <param name="idEntityField">Идентификатор поля, для которого открываем форму выбора (idParent)</param>
        /// <param name="idOwnerValue">Идентификатор элемента вышестоящей сущности (в которой находится ТЧ с полем idParent)</param>
		public AddFilterOnTablepartOwnerFieldForParentLink(int idEntityField, int idOwnerValue)
		{
			_idEntityField = idEntityField;
			_idOwnerValue = idOwnerValue;
		}

		#region Implementation of ITSqlStatementDecorator

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
            _builder = (queryBuilder as ISelectQueryBuilder);
			SelectStatement result = (source as SelectStatement);
			if (_builder == null || result==null)
				return source;
			_entity = _builder.Entity;
			if (_entity.EntityType != EntityType.Tablepart || !_entity.Fields.Any(a => a.Id == _idEntityField && a.IdEntityLink == a.IdEntity))
				return source;

            #region вычисляем имя idowner
            //var dc = IoC.Resolve<DbContext>().Cast<DataContext>();
			var ownerField = _entity.Fields.SingleOrDefault(
				a =>
				a.IdEntityLink.HasValue &&
				a.EntityLink.Fields.Any(
					b => b.IdEntityLink.HasValue && b.Entity.EntityType != EntityType.Tablepart && b.IdEntityLink == _entity.Id));
			if (ownerField==null)
				return source;
			/*
			var ownerEntityId = dc.EntityField.Where(ef => ef.IdEntityLink.HasValue && 
                                                           ef.IdEntityLink.Value == _entity.Id &&
                                                           ef.IdEntity != _entity.Id).Select(ef => ef.IdEntity).FirstOrDefault();

            if (ownerEntityId == 0)
                return source;

            var ownerFieldName = _entity.Fields.Where(f => f.IdEntityLink.HasValue && 
                                                           f.IdEntityLink == ownerEntityId)
                                               .Select(f => f.Name).FirstOrDefault();*/
            #endregion

            FilterConditions filterConditions = new FilterConditions
				{
                    Field = ownerField.Name,
					Operator = ComparisionOperator.Equal,
                    Value = _idOwnerValue
				};
			AddWhere addWhere = new AddWhere(filterConditions, LogicOperator.And, true);
			result = (addWhere.Decorate(result, queryBuilder) as SelectStatement);
			//this.Log(result, queryBuilder);
			return result;
		}

		#endregion
	}
}
