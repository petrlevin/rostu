using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.BusinessLogic.Denormalizer.Decorator;
using Platform.BusinessLogic.Denormalizer.ModelTransformers;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.SummaryAggregates
{
    /// <summary>
    /// Итоговые строки гридов: Анализирует сущность на предмет наличия полей, для которых указана агретатная функция.
    /// </summary>
    public class AggregatesAnalyzer
    {
        #region Singleton

        private static AggregatesAnalyzer _instance;

        private AggregatesAnalyzer() { }

        private static AggregatesAnalyzer instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new AggregatesAnalyzer();
                }
                return _instance;
            }
        }

        #endregion

        #region Public Statics

        /// <summary>
        /// Есть ли среди полей сущности <paramref name="idEntity"/> хотябы одно поле, для которого указана агрегатная функция 
        /// (в справочнке "Настройки полей сущности").
        /// </summary>
        /// <param name="idEntity"></param>
        /// <param name="inFields"></param>
        /// <returns></returns>
        public static bool Any(int idEntity, IEnumerable<IEntityFieldInfo> inFields = null)
        {
            return instance.any(idEntity, inFields);
        }

        /// <summary>
        /// Для сущности с идентификатором <paramref name="entityId"/> возвращает коллекцию полей, для которых указана агрегатная функция. 
        /// При этом, если данная сущность является родительской в ДТЧ, то анализируется дочерняя сущность.
        /// Возвращаются ресурсные поля, размноженные по периодам. 
        /// </summary>
        /// <exception cref="PlatformException">Если не зарегистрирован DenormalizerDecorator, то выбрасывается исключение. 
        /// Если вам нужны реальные поля, обратитесь к методу GetRealAggregates.
        /// </exception>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static IEnumerable<IAggregateInfo> GetAggregates(int entityId)
        {
            return instance.getAggregates(entityId);
        }

        #endregion


        #region Instance Methods - методы экземпляра, копии публичных статичных методов

        /// <summary>
        /// Есть ли среди полей сущности <paramref name="idEntity"/> хотябы одно поле, для которого указана агрегатная функция 
        /// (в справочнке "Настройки полей сущности").
        /// </summary>
        /// <param name="idEntity"></param>
        /// <param name="inFields"></param>
        /// <returns></returns>
        private bool any(int idEntity, IEnumerable<IEntityFieldInfo> inFields = null)
        {
            return getRealAggregates(idEntity).Any(agg =>
                    inFields == null
                    || inFields.Any(fi => fi.EntityFieldName == agg.Field)
                );
        }

        /// <summary>
        /// Для сущности с идентификатором <paramref name="entityId"/> возвращает коллекцию полей, для которых указана агрегатная функция. 
        /// При этом, если данная сущность является родительской в ДТЧ, то анализируется дочерняя сущность.
        /// Возвращаются реальные ресурсные поля. Чтобы получить размноженные по периодам ресурсные поля, используйте метод GetAggregates
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private IEnumerable<IAggregateInfo> getRealAggregates(int entityId)
        {
            entityId = getTargetEntityId(entityId);
            return getAggregatesForEntity(entityId);
        }

        /// <summary>
        /// Для сущности с идентификатором <paramref name="entityId"/> возвращает коллекцию полей, для которых указана агрегатная функция. 
        /// При этом, если данная сущность является родительской в ДТЧ, то анализируется дочерняя сущность.
        /// Возвращаются ресурсные поля, размноженные по периодам. 
        /// </summary>
        /// <exception cref="PlatformException">Если не зарегистрирован DenormalizerDecorator, то выбрасывается исключение. 
        /// Если вам нужны реальные поля, обратитесь к методу GetRealAggregates.
        /// </exception>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private IEnumerable<IAggregateInfo> getAggregates(int entityId)
        {
            int targetEnittyId = entityId;
            var isDtpMaster = isMasterTablepart(entityId, out targetEnittyId);
            IEnumerable<IAggregateInfo> aggregates = getAggregatesForEntity(targetEnittyId);
            if (isDtpMaster)
            {
                DenormalizedTablepartAnalyzer denormTpAnalyzer = denormInfo.GetAnalyzerByMaster(entityId);
                IEnumerable<int> periods = getDenormalizerPeriods();
                List<IAggregateInfo> aggregatesByPeriods = new List<IAggregateInfo>();
                foreach (IAggregateInfo aggregateInfo in aggregates)
                {
                    foreach (int periodId in periods)
                    {
                        IEntityField configuredField = denormTpAnalyzer.getConfiguredValueField(
                                denormTpAnalyzer.ValueFields.Single(vf => vf.Name == aggregateInfo.Field), 
                                periodId);
                        aggregatesByPeriods.Add(new AggregateInfo() { Field = configuredField.Name, Function = aggregateInfo.Function });
                    }
                }
                return aggregatesByPeriods;
            }
            return aggregates;
        }

        #endregion 

        #region Private Members

        private DataContext db
        {
            get { return IoC.Resolve<DbContext>().Cast<DataContext>(); }
        }

        private DenornalizedEntitiesInfo denormInfo
        {
            get
            {
                return IoC.Resolve<DenornalizedEntitiesInfo>();
            }
        }

        /// <summary>
        /// Возвращает агрегаты для указанной сущности
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private IEnumerable<IAggregateInfo> getAggregatesForEntity(int entityId)
        {
            return db.EntityFieldSetting.Include(efs => efs.EntityField).Where(efs =>
                    efs.EntityField.IdEntity == entityId
                    && efs.IdAggregateFunction.HasValue
                )
                .ToList()
                .Select(efs => efs.ToAggregateInfo());
        }

        /// <summary>
        /// Является ли сущность <paramref name="entityId"/> родительской в ДТЧ ? 
        /// Если да, то в <paramref name="childId"/> возвращается id дочерней сущности ДТЧ.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="childId"></param>
        /// <returns></returns>
        private bool isMasterTablepart(int entityId, out int childId)
        {
            childId = getTargetEntityId(entityId);
            if (entityId == childId)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Если сущность <paramref name="entityId"/> является родительской в ДТЧ, то возвращается id дочерней сущности.
        /// Иначе возвращается <paramref name="entityId"/>.
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private int getTargetEntityId(int entityId)
        {
            if (denormInfo.IsMasterTablepart(entityId))
            {
                return denormInfo.GetAnalyzerByMaster(entityId).ChildTp.Id;
            }
            return entityId;
        }

        private IEnumerable<int> getDenormalizerPeriods()
        {
            DenormalizerDecorator denormalizerDecorator = IoC.Resolve<List<TSqlStatementDecorator>>("Decorators")
                .Where(d => d is DenormalizerDecorator)
                .Cast<DenormalizerDecorator>()
                .SingleOrDefault();

            if (denormalizerDecorator == null)
            {
                throw new PlatformException("Для получения ресурсных полей, размноженных по периодам, в системе должен быть зарегистрирован декоратор DenormalizerDecorator. Однако получить из контейнера его не удалось.");
            }

            return denormalizerDecorator.DenormalizedPeriods;
        }

        #endregion
    }
}
