using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.DbEnums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.SummaryAggregates;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.PrimaryEntities.Reference;
namespace Platform.BusinessLogic.Reference
{
    /// <summary>
    /// Настройки полей сущности
    /// </summary>
    public partial class EntityFieldSetting
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAggregateInfo ToAggregateInfo()
        {
            if (AggregateFunction.HasValue)
                return new _AggregateInfo(this);
            return null;
        }


        class _AggregateInfo : IAggregateInfo
        {
            private EntityFieldSetting _entityFieldSetting;

            private List<EntityFieldType> allowedValuefieldTypes = new List<EntityFieldType>
                {
                    EntityFieldType.Numeric,
                    EntityFieldType.TinyInt,
                    EntityFieldType.SmallInt,
                    EntityFieldType.Int,
                    EntityFieldType.BigInt,
                    EntityFieldType.Money
                };

            public _AggregateInfo(EntityFieldSetting entityFieldSetting)
            {
                _entityFieldSetting = entityFieldSetting;
                if (!allowedValuefieldTypes.Contains(_entityFieldSetting.EntityField.EntityFieldType))
                    throw new PlatformException("В качестве поля аггрегации можно указывать только числовое поле");
            }

            public string Field
            {
                get { return _entityFieldSetting.EntityField.Name; }
            }

            public AggregateFunction Function
            {
                get
                {
                    return _entityFieldSetting.AggregateFunction.Value;
                }
            }
        }
    }
}