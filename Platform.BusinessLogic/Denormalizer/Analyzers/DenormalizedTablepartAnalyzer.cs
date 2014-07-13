using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Common;
using Platform.Utils.LazyProperties;

namespace Platform.BusinessLogic.Denormalizer.Analyzers
{
    public class DenormalizedTablepartAnalyzer
    {
        /// <summary>
        /// Идентификатор сущности "Иерархия периодов"
        /// </summary>
        private const int hierarchyPeriodId = -1879048152;

        private const string columnNameMask = "{0}{{{1}}}"; // ИмяПоля{idПериода}

        public static string GetColumnNameBy(string valueFieldName, int periodId)
        {
            return String.Format(columnNameMask, valueFieldName, periodId);
        }

        /// <summary>
        /// Сущность дочерней табличной части (как исходные данные для определения остальной информации).
        /// </summary>
        public Entity ChildTp { get; private set; }
        
        private DataContext db { get; set; }

        /// <summary>
        /// Поля <see cref="ChildTp"/>
        /// </summary>
        private IEnumerable<EntityField> childTpFields
        {
            get { return db.EntityField.Where(ef => ef.IdEntity == ChildTp.Id); }
        }

        /// <summary>
        /// Анализатор структуры табличной части, которая подлежит денормализации
        /// </summary>
        /// <param name="entity">Сущность табличной части</param>
        /// <param name="db"></param>
        public DenormalizedTablepartAnalyzer(Entity entity, DataContext db)
		{
            this.db = db;
			ChildTp = entity;
            initLazyAnalyzers();
		}

        private Dictionary<string, ILazyPropertyAnalyzer> lazyAnalyzers;

        #region Public Members

        /// <summary>
        /// Проверка допустимости денормализации для данной ТЧ.
        /// Должны быть только эти колонки: id, idOwner, idMaster, idHierarchyPeriod, value.
        /// </summary>
        /// <returns></returns>
        public void StuctureShouldBeCorrect()
        {
            fetchAllValues();

            //if (childTpFields.Count() != 5)
            //    throw new PlatformException(String.Format("В дочерней таблице {0} должно быть 5 полей", ChildTp.Name));

            StringBuilder err = new StringBuilder();

            var analyzers = lazyAnalyzers.Where(kvp => kvp.Value.IsRequired() && !kvp.Value.HasValue());
            foreach (KeyValuePair<string, ILazyPropertyAnalyzer> keyValuePair in analyzers)
            {
                err.AppendLine(String.Format("Свойство {0} не получило значения", keyValuePair.Key));
            }

            if (err.Length != 0)
                throw new PlatformException(err.ToString());
        }

        /// <summary>
        /// Возвращает настроенное значимое поле для указанного периода
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="periodId"></param>
        /// <param name="caption"></param>
        /// <param name="valueFieldsNames">Если параметр указан, то возвращаются значимые поля, соответствующие только данным ресурсным полям</param>
        /// <returns></returns>
        public IEnumerable<IEntityField> GetConfiguredValueFields(int entityId, ColumnsInfo columns)
        {
            IEnumerable<EntityField> fields = ValueFields;
            if (columns.Resources != null)
                fields = fields.Where(a => columns.Resources.Contains(a.Name));

            return 
                columns.Periods.SelectMany(period =>
                    fields.Select(vf => 
                        getConfiguredValueField(vf, period.PeriodId, entityId, period.Caption)));
        }

        public IEnumerable<IEntityField> GetConfiguredValueFields(IEnumerable<int> periodIds)
        {
            return periodIds.SelectMany(pid => ValueFields.Select(vf => getConfiguredValueField(vf, pid)));
        }


        public IEntityField getConfiguredValueField(IEntityField valueField, int periodId, int? entityId = null, string caption = null)
        {
            if (entityId == null)
                entityId = valueField.IdEntity;

            if (caption == null)
                caption = periodId.ToString();

            string captionFormat = "{0} {1}"; // {0} = имя поля, {1} = имя периода, полученное от IColumnFactoryForDenormalizedTablepart.GetColumns
            if (ValueFields.Count() == 1)
                captionFormat = "{1}";

            return new EntityField()
            {
                Id = 0,
                Name = GetColumnNameBy(valueField.Name, periodId),
                Caption = string.Format(captionFormat, valueField.Caption, caption),
                IdEntity = (int)entityId,
                AllowNull = true,

                IdEntityFieldType = valueField.IdEntityFieldType,
                ReadOnly = valueField.ReadOnly,
                Size = valueField.Size,
                Precision = valueField.Precision,
                RegExpValidator = valueField.RegExpValidator
            };
        }

        #endregion

        #region Вычисляемые значения

        /// <summary>
        /// Сущность табличной части, являющейся родительской по отношению к денормализованной
        /// </summary>
        public IEntity MasterTp { get { return (IEntity)lazyAnalyzers["MasterTp"].GetValue(); } }
        
        /// <summary>
        /// Табличное поле, соответствующее <see cref="MasterTp"/>
        /// </summary>
        public IEntityField MasterTablefield { get { return (IEntityField)lazyAnalyzers["MasterTablefield"].GetValue(); } }

        /// <summary>
        /// Табличное поле, соответствующее <see cref="ChildTp"/>
        /// </summary>
        public IEntityField ChildTablefield { get { return (IEntityField)lazyAnalyzers["ChildTablefield"].GetValue(); } }

        /// <summary>
        /// Ссылка на владельца в родительской ТЧ
        /// </summary>
        public IEntityField OwnerInMasterTpField { get { return (IEntityField)lazyAnalyzers["OwnerInMasterTpField"].GetValue(); } }

        /// <summary>
        /// Сущность-владелец - сущность, в которой находится денормализованная ТЧ
        /// </summary>
        public IEntity OwnerEntity { get { return (IEntity) lazyAnalyzers["OwnerEntity"].GetValue(); } }

        // Поля дочерней ТЧ

        /// <summary>
        /// Поле-идентификатор внутри ТЧ <see cref="ChildTp"/>
        /// </summary>
        private IEntityField IdField { get { return (IEntityField)lazyAnalyzers["IdField"].GetValue(); } }

        /// <summary>
        /// Ссылка на владельца в ТЧ <see cref="ChildTp"/>
        /// </summary>
        public IEntityField OwnerField { get { return (IEntityField)lazyAnalyzers["OwnerField"].GetValue(); } }

        /// <summary>
        /// Ссылка на родительскую ТЧ в <see cref="ChildTp"/>
        /// </summary>
        public IEntityField MasterField { get { return (IEntityField)lazyAnalyzers["MasterField"].GetValue(); } }

        /// <summary>
        /// Сссылка на справочник периодов в ТЧ <see cref="ChildTp"/>
        /// </summary>
        public IEntityField HierarchyPeriodField { get { return (IEntityField)lazyAnalyzers["HierarchyPeriodField"].GetValue(); } }

        /// <summary>
        /// Поля с числовым значением в ТЧ <see cref="ChildTp"/>
        /// </summary>
        public List<EntityField> ValueFields { get { return (List<EntityField>)lazyAnalyzers["ValueFields"].GetValue(); } }

        #endregion

        private void initLazyAnalyzers()
        {
            lazyAnalyzers = new Dictionary<string, ILazyPropertyAnalyzer>();

            // Для удобства восприятия анализаторы расположены в порядке вычисления

            lazyAnalyzers["ChildTablefield"] = new LazyPropertyAnalyzer<IEntityField>(getChildTablefield) { IsRequired = true };

            lazyAnalyzers["OwnerEntity"] = new LazyPropertyAnalyzer<IEntity>(getOwnerEntity) {IsRequired = true};

            // поля ChildTp
            lazyAnalyzers["IdField"] = new LazyPropertyAnalyzer<IEntityField>(getIdField) {IsRequired = true};
            lazyAnalyzers["OwnerField"] = new LazyPropertyAnalyzer<IEntityField>(getOwnerField) { IsRequired = true };
            lazyAnalyzers["HierarchyPeriodField"] = new LazyPropertyAnalyzer<IEntityField>(getHierarchyPeriodField) { IsRequired = true };
            lazyAnalyzers["MasterField"] = new LazyPropertyAnalyzer<IEntityField>(getMasterField) { IsRequired = true };
            lazyAnalyzers["ValueFields"] = new LazyPropertyAnalyzer<List<EntityField>>(getValueFields) { IsRequired = true };

            lazyAnalyzers["MasterTp"] = new LazyPropertyAnalyzer<IEntity>(getMasterTp) { IsRequired = true };
            lazyAnalyzers["MasterTablefield"] = new LazyPropertyAnalyzer<IEntityField>(getMasterTablefield) { IsRequired = true };
            lazyAnalyzers["OwnerInMasterTpField"] = new LazyPropertyAnalyzer<IEntityField>(getOwnerInMasterTpField) { IsRequired = true };
            //lazyAnalyzers["LinkingFilter"] = new LazyPropertyAnalyzer<Filter>(getLinkingFilter);
        }

        #region Процедуры получения информации

        private IEntityField getChildTablefield()
        {
            return db.EntityField.SingleOrDefault(ef =>
                ef.IdEntityFieldType == (byte)EntityFieldType.Tablepart
                && ef.IdEntityLink == ChildTp.Id);
        }

        private IEntity getOwnerEntity()
        {
            return db.Entity.SingleOrDefault(e => e.Id == MasterTablefield.IdEntity);
        }

        // Поля в ChildTp

        private IEntityField getIdField()
        {
            return childTpFields.SingleOrDefault(f =>
                f.Name == "id"
                && f.EntityFieldType == EntityFieldType.Int);
        }

        private IEntityField getOwnerField()
        {
            return db.EntityField.SingleOrDefault(ef => ef.Id == ChildTablefield.IdOwnerField);
        }

        private IEntityField getHierarchyPeriodField()
        {
            return childTpFields.SingleOrDefault(f =>
                f.IdEntityLink.HasValue
                && f.IdEntityLink == hierarchyPeriodId);
        }

        private IEntityField getMasterField()
        {
            return childTpFields.SingleOrDefault(f =>
                f.IdEntityFieldType == (byte)EntityFieldType.Link
                && f.IdEntityLink != hierarchyPeriodId
                && f.Id!= OwnerField.Id);
        }

        private List<EntityField> getValueFields()
        {
            byte[] valueFieldTypes = new[]
                {
                    (byte) EntityFieldType.Int,
                    (byte) EntityFieldType.TinyInt,
                    (byte) EntityFieldType.BigInt,
                    (byte) EntityFieldType.Numeric,
                    (byte) EntityFieldType.Money
                };

            return childTpFields.Where(f => f.Id != IdField.Id && valueFieldTypes.Contains(f.IdEntityFieldType)).ToList();
        }

        //

        private IEntity getMasterTp()
        {
            return db.Entity.SingleOrDefault(e => e.Id == MasterField.IdEntityLink);
        }

        private IEntityField getMasterTablefield()
        {
            return db.EntityField.SingleOrDefault(ef =>
                ef.IdEntityFieldType == (byte)EntityFieldType.Tablepart
                && ef.IdEntityLink == MasterTp.Id);
        }
        
        private IEntityField getOwnerInMasterTpField()
        {
            return db.EntityField.SingleOrDefault(ef => ef.Id == MasterTablefield.IdOwnerField);
        }

        #endregion

        #region Private Methods

        private void fetchAllValues()
        {
            foreach (KeyValuePair<string, ILazyPropertyAnalyzer> lazyAnalyzer in lazyAnalyzers)
            {
                lazyAnalyzer.Value.GetValue();
            }
        }

        #endregion
    }
}
