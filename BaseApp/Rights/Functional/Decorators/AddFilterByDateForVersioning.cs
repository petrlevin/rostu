using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.EntityTypes.Interfaces;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Rights.Functional.Decorators
{
    /// <summary>
    /// Фильтровать элементы при выполнении 
    /// </summary>
    public class AddFilterByDateForVersioning : TSqlStatementDecorator
    {
        private ISelectQueryBuilder _builder;

        private IEntity _entity;
        private Entity _fromEntity;

        private readonly int _idEntityGenericLink;
        private readonly int _idEntityField;

        private DateTime? _actualDate;

        private const string ValidityFrom = "ValidityFrom";
        private const string ValidityTo = "ValidityTo";

        //Поле на выбор которого накладываем декоратор
        private EntityField _entityField;

        private int? _idDoc;

        /// <summary>
        /// Список типов поля - общих ссылок
        /// </summary>
        private static readonly EntityFieldType[] _genericLink = new[]
				{
					EntityFieldType.ReferenceEntity,
					EntityFieldType.ToolEntity,
					EntityFieldType.TablepartEntity,
					EntityFieldType.DocumentEntity
				};

        #region Implementation of ITSqlStatementDecorator

        public AddFilterByDateForVersioning(int idEntityField, int idEntity, FieldValues fieldValues, int? idDoc)
        {
            _idEntityField = idEntityField;
            _idEntityGenericLink = idEntity;

            _entityField = Objects.ById<EntityField>(_idEntityField);
            _fromEntity = Objects.ById<Entity>(_entityField.IdEntity);

            //Если текущая сущность -- документ, пытаемся инициализировать дату значением, пришедшим с клиента
            if (_fromEntity.EntityType == EntityType.Document)
            {
                var dateFieldId =
                    _fromEntity.Fields.Where(f => f.Name.ToLower() == "date").Select(f => f.Id).FirstOrDefault();

                DateTime date;
                if (fieldValues.ContainsKey(dateFieldId) &&
                    DateTime.TryParse(fieldValues[dateFieldId].ToString(), out date))
                    _actualDate = date;
                else
                    _actualDate = null;
            }
            else
                _idDoc = idDoc;
        }

        /// <summary>
        /// Применить декоратор
        /// </summary>
        /// <param name="source">Расширяемое выражение</param>
        /// <param name="queryBuilder">Построитель</param>
        /// <returns>TSqlStatement</returns>
        protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
        {
            if (_fromEntity.EntityType != EntityType.Document && _fromEntity.EntityType != EntityType.Tablepart)
                return source;

            if (_fromEntity.EntityType == EntityType.Document && !_actualDate.HasValue)
                return source;

            //Если ТЧ -- получаем дату из базы
            if (_fromEntity.EntityType == EntityType.Tablepart)
            {
                if (!_idDoc.HasValue)
                    return source;

                var dc = IoC.Resolve<DbContext>().Cast<DataContext>();
                var documentEntityId = dc.EntityField.Where(ef => ef.IdEntityLink.HasValue &&
                                                                  ef.IdEntityLink.Value == _fromEntity.Id &&
                                                                  ef.IdEntity != _fromEntity.Id)
                                                     .Select(ef => ef.IdEntity).FirstOrDefault();
                
                if (documentEntityId == 0)
                    return source;
                
                var documentEntity = Objects.ById<Entity>(documentEntityId);
                if (documentEntity == null || documentEntity.IdEntityType != (int)EntityType.Document)
                    return source;

                _actualDate = dc.Database.SqlQuery<DateTime>(String.Format("Select [Date] From [{0}].[{1}] Where [id] = {2}", documentEntity.Schema, documentEntity.Name, _idDoc.Value)).FirstOrDefault();
                 
            }


            //Сущность на которую ссылается поле
            Entity genericEntity = Objects.ById<Entity>(_idEntityGenericLink);
            
            //Если поле не ссылка
            if (!_genericLink.Contains(_entityField.EntityFieldType) && _entityField.EntityLink == null)
                return source;
            
            //Если ссылка ссылается на неверсионную сущность
            if (!_genericLink.Contains(_entityField.EntityFieldType) && (_entityField.EntityLink.EntityType != EntityType.Reference || !_entityField.EntityLink.IsVersioning))
                return source;
            
            //Если мультиссылка ссылается на неверсионную сущность
            if (_genericLink.Contains(_entityField.EntityFieldType) && !genericEntity.IsVersioning)
                return source;
            
            _builder = (queryBuilder as ISelectQueryBuilder);
            if ( _entity == null && ( _builder == null || (_entity = _builder.Entity) == null))
                return source;
            
            //Родительский And
            var holderFilterConditions = new FilterConditions {Type = LogicOperator.And};

            //Условия на ValidityFrom
            var validityFromHolder = new FilterConditions {Type = LogicOperator.Or};

            var validityFromIsNull = new FilterConditions
                {
                    Type = LogicOperator.Simple,
                    Field = ValidityFrom,
                    Operator = ComparisionOperator.IsNull
                };

            var validityFromLessThenDate = new FilterConditions
                {
                    Type = LogicOperator.Simple,
                    Field = ValidityFrom,
                    Operator = ComparisionOperator.LessOrEqual,
                    Value = _actualDate
                };

            validityFromHolder.Operands = new List<IFilterConditions>(){ validityFromIsNull, validityFromLessThenDate};

            //Условия на ValidityTo
            var validityToHolder = new FilterConditions {Type = LogicOperator.Or};

            var validityToIsNull = new FilterConditions
            {
                Type = LogicOperator.Simple,
                Field = ValidityTo,
                Operator = ComparisionOperator.IsNull
            };

            var validityToGreateThenDate = new FilterConditions
            {
                Type = LogicOperator.Simple,
                Field = ValidityTo,
                Operator = ComparisionOperator.Greater,
                Value = _actualDate
            };

            validityToHolder.Operands = new List<IFilterConditions>() { validityToIsNull, validityToGreateThenDate };


            //Получаем итоговое условие
            holderFilterConditions.Operands = new List<IFilterConditions>(){ validityFromHolder, validityToHolder };
            var addWhere = new AddWhere(holderFilterConditions, LogicOperator.And, true);
            
            source = addWhere.Decorate(source, _builder);

            return source;
        }
        #endregion

    }
}
