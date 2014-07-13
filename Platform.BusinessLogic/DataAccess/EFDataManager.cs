using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// менеджер использует Entity Framework
    /// </summary>
    public class EFDataManager : DataManager
    {

        #region Конструкторы
        /// <summary>
        /// cоздает менеджер использующий Entity Framework
        /// 
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="source"></param>
        public EFDataManager(SqlConnection dbConnection, IEntity source)
            : this(dbConnection, source, null)
        {

        }

        //Дополнительные конструкторы



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="source"></param>
        /// <param name="controlLauncher"></param>

        /// <param name="dbContext"></param>
        public EFDataManager(SqlConnection dbConnection, IEntity source, DbContext dbContext, EntityManager entityManager = (EntityManager)null)
            : base(dbConnection, source)
        {
            _dbContext = dbContext ?? IoC.Resolve<DbContext>();
            _entityManager = entityManager ?? new EntityManager(source);
        }


        public EFDataManager(SqlConnection dbConnection, IEntity source, EntityManager entityManager = (EntityManager)null)
            : base(dbConnection, source)
        {
            _dbContext = IoC.Resolve<DbContext>();
            _entityManager = entityManager ?? new EntityManager(source);
        }
        





        #endregion


        #region Override


        protected override int? DoCreateEntry(Dictionary<String, object> values)
        {
            base.DoCreateEntry(values);
            ValidateRequiredField(values);

            var businessEntity = CreateBusinessEntity(EntityState.Added);
            if (!(businessEntity is IIdentitied))
                throw new PlatformException(
                    String.Format(
                        "Тип  '{0}' не потдерживает интерфейс '{1}'. Создание нового элемента не возможно. ",
                        ObjectContext.GetObjectType(businessEntity.GetType()), typeof(IIdentitied)));

            businessEntity.SetValues(values, true, true);

            using (new ControlScope(businessEntity))
            {
                SaveChanges();
            }


            //SaveChanges();

            OnEntityCreated(businessEntity);

            return ((IIdentitied)businessEntity).Id;
        }

        protected virtual void OnEntityCreated(IBaseEntity businessEntity)
        {

        }

        protected override int DoUpdateEntry(int itemId, Dictionary<string, object> values)
        {
            var businessEntity = UpdateEntity(itemId, values);

            return ((IIdentitied)businessEntity).Id;
        }

        protected override void DoUpdateEntries(int[] itemIds, Dictionary<string, object> values)
        {
            var updateErrors = new List<String>();

            foreach (var itemId in itemIds)
            {
                try
                {
                    UpdateEntity(itemId, values, true);
                }
                catch (ControlResponseException ex)
                {
                    var itemCaption = DbContext.Database.SqlQuery<string>(String.Format("Select dbo.GetCaption({0},{1})", Source.Id, itemId)).FirstOrDefault();
                    updateErrors.Add( itemCaption + " - контроль №" + ex.UNK);
                }
            }

            if (updateErrors.Any())
                throw new ControlResponseException("Изменение следующих записей не удовлетворяет контролям: <br/>" + string.Join(", <br/>", updateErrors), null, Source);
        }

        protected IBaseEntity UpdateEntity(int itemId, Dictionary<string, object> values, bool skipSkippable = false)
        {
            var businessEntity = LoadBusinessEntity(itemId);

            return DoUpdateEntity(values, businessEntity, skipSkippable);
        }

        protected IBaseEntity DoUpdateEntity(Dictionary<string, object> values, IBaseEntity businessEntity, bool skipSkippable = false)
        {
            CheckValues(values);
            ValidateRequiredField(values, true);

            List<string> reanOnlyFields =
                Source.Fields.Cast<EntityField>().Where(a => a.ReadOnly.HasValue && a.ReadOnly.Value).Select(a => a.Name).
                       ToList();
            values =
                values.Where(a => reanOnlyFields.All(b => !b.Equals(a.Key, StringComparison.OrdinalIgnoreCase))).ToDictionary(
                    a => a.Key, b => b.Value);

            var actualType = ObjectContext.GetObjectType(businessEntity.GetType());
            if (!(businessEntity is IIdentitied))
                throw new PlatformException(
                    String.Format(
                        "Тип  '{0}' не поддерживает интерфейс '{1}'.Запись элемента не возможна. ",
                        actualType, typeof (IIdentitied)));
            businessEntity.SetValues(values, true);

            DbContext.Entry(businessEntity).State = EntityState.Modified;


            using (new ControlScope(businessEntity, skipSkippable))
            {
                SaveChanges();
            }

            return businessEntity;
        }

        private int SaveChanges()
        {
                return DbContext.SaveChanges();
        }


        protected override bool DoDeleteItem(int[] itemIds)
        {
            foreach (int itemId in itemIds)
            {
                var businessEntity = LoadBusinessEntity(itemId);
                DoDeleteItem(businessEntity);
            }
            return true;
        }

        protected virtual void DoDeleteItem(IBaseEntity businessEntity)
        {
            
            if (!(businessEntity is IIdentitied))
                throw new PlatformException(
                    String.Format(
                        "Тип  '{0}' не потдерживает интерфейс '{1}'. Удаление элемента не возможно. ",
                        ObjectContext.GetObjectType(businessEntity.GetType()), typeof (IIdentitied)));
            RemoveBusinessEntity(businessEntity);
            using (new ControlScope(businessEntity))
            {
                SaveChanges();
            }
        }

		public override int CreateNewVersionEntityItem(int idItem)
        {
            IBaseEntity source = LoadBusinessEntity(idItem);
            if (source == null)
                throw new PlatformException("Не найден элемент сущности '" + Source.Name + "' с идентификатором - " + idItem.ToString());

			int idRoot;
			if (source.GetValue("IdRoot") != null)
				idRoot = (int)source.GetValue("IdRoot");
			else
				idRoot = (int)source.GetValue("Id");

			DateTime validityTo = Convert.ToDateTime(source.GetValue("ValidityTo") ?? DateTime.MaxValue.Date);
			if (_entityManager.AsQueryable<IVersioning>().Any(a => a.IdRoot.HasValue && a.IdRoot == idRoot && (a.ValidityFrom ?? DateTime.MinValue.Date) > validityTo))
			{
				throw new PlatformException("Можно создавать только на основе актуального элемента");
			}

			DateTime date = DateTime.Now.Date;
			if (date > validityTo)
				date = validityTo;
            source.SetValue("ValidityTo", date);


            int result;
            using (TransactionScope transaction = CreateTransaction())
            {
                Clone cloner = new Clone(source);
                object target = cloner.GetResult();
                //target.SetValue("ValidityFrom", date.AddSeconds(1));
				//target.SetValue("ValidityTo", Convert.ToDateTime("31.12.9999"));
				target.SetValue("ValidityFrom", date);
				target.SetValue("ValidityTo", null);
                target.SetValue("IdRoot", idRoot);
                _dbContext.Entry(target).State = EntityState.Added;
                result = SaveChanges();
                //if (result < 2)
                    //throw new PlatformException("Идентификатор новой или обновляенной записи не был получен");
                transaction.Complete();
            }
            return result;
        }

        public override Dictionary<string, object> GetDefaults()
        {

            var result = base.GetDefaults();
            var businessEntity = CreateBusinessEntity(EntityState.Detached);

            var initNew = businessEntity as IInitNew;
            if (initNew == null)
                return result;
            businessEntity.SetValues(result);
            initNew.Init(DbContext);
            return businessEntity.GetScalarValues(true);

        }


        protected override int? DoCloneInternalPartEntry(object sourceItem, object targetItem)
        {
            //получить все ТЧ
            List<EntityField> fieldsTp =
                Source.Fields.Cast<EntityField>().Where(
                    a => a.EntityFieldType == EntityFieldType.Tablepart && a.IdEntityLink.HasValue).ToList();
            List<int> entitiesId = fieldsTp.Select(a => a.IdEntityLink.Value).ToList();

            List<EntityField> fieldsTpStart =
                fieldsTp.Where(
                    a => !a.EntityLink.Fields.Any(b => entitiesId.Where(c => c != a.IdEntityLink).Contains(b.IdEntityLink.Value))).ToList();
            foreach (EntityField entityField in fieldsTpStart)
            {
                sourceItem.GetValue(entityField.Name);
            }

            //получить все ТЧ и мультлинки, который ссылаются только на документ
            /*
            List<EntityField> fieldsTpMl =
                Source.Fields.Cast<EntityField>().Where(
                    a => a.EntityFieldType == EntityFieldType.Tablepart || a.EntityFieldType == EntityFieldType.Multilink).ToList();
            List<int> entitiesId = fieldsTpMl.Select(a => a.IdEntityLink.Value).ToList();
            var fieldsTpMlStart =
                fieldsTpMl.Where(
                    a => !a.EntityLink.Fields.Any(b => entitiesId.Where(c => c != a.IdEntityLink).Contains(b.IdEntityLink.Value)));
             */
            throw new NotImplementedException();
        }

        #endregion


        #region Private



        private readonly DbContext _dbContext;

        private readonly EntityManager _entityManager;





        /// <summary>
        /// Проверка данных, поступивших с клиента, на наличие данных для обязательных полей
        /// </summary>
        /// <param name="values">Данные поступившие с клиента</param>
        /// <param name="onlyExistedInSource">Проверять только поля присутствующиев словаре значений</param>
        /// <returns></returns>
        protected virtual void ValidateRequiredField(Dictionary<String, object> values, bool onlyExistedInSource = false)
        {
            var unfilled = new List<string>();
            foreach (EntityField field in Source.RealFields.Cast<EntityField>().Where(a => !a.ColumnAllowNull && a.Name != "id"))
            {
                object value;
                if (values.TryGetValue(field.Name, out value))
                {
                    if (value == null || string.IsNullOrEmpty(Convert.ToString(value)))
                    {
                        unfilled.Add(field.Caption);
                    }

                }
                else if ((!onlyExistedInSource) && (field.EntityFieldType != EntityFieldType.Bool))
                {
                    unfilled.Add(field.Caption);
                }
            }
            if (unfilled.Any())
                throw new FormValidationException("Обязательные поля",String.Format("В форме присутствуют незаполненные обязательные поля:<br/> {0}", unfilled.ToString("<br/>")));

        }

        private IBaseEntity CreateBusinessEntity(EntityState state)
        {
            return _entityManager.Create(state);
        }

        private void RemoveBusinessEntity(IBaseEntity businessEntity)
        {
            _entityManager.Remove(businessEntity);
        }


        #endregion


        #region Protected

        protected IBaseEntity LoadBusinessEntity(int itemId)
        {
            
            return LoadBusinessEntity(itemId, _dbContext);
            
        }


        protected IBaseEntity LoadBusinessEntity(int itemId, DbContext dbContext)
        {

            if (_entityManager.Is<IIdentitied>())
                return (IBaseEntity) _entityManager.AsQueryable<IIdentitied>().First(i => itemId == i.Id);
            else
                return _entityManager.Find(itemId);
            //var result = (IBaseEntity)_entityManager.AsQueryable<IIdentitied>().First(i => itemId == i.Id);
            //((IObjectContextAdapter)DbContext).ObjectContext.Refresh(RefreshMode.ClientWins,result );
            //return result;
        }

        protected DbContext DbContext
        {
            get { return _dbContext; }
        }

        #endregion

    }
}
