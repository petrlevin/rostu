using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Denormalizer.Analyzers
{
    public abstract class EntityAnalyzer
    {
        private DenornalizedEntitiesInfo denormInfo { get; set; }

        protected DenormalizedTablepartAnalyzer _tpAnalyzer;
        protected DenormalizedTablepartAnalyzer TpAnalyzer
        {
            get
            {
                if (_tpAnalyzer == null)
                {
                    throw new PlatformException("Ожидается, что использование анализатора ТЧ (TpAnalyzer), возможно только когда TargetEntity является либо дочерней, либо родительской сущностью денормализованной ТЧ");
                }
                return _tpAnalyzer;
            }
        }

        /// <summary>
        /// Сущность табличной части
        /// </summary>
        protected Entity TargetEntity { get; set; }

        public EntityAnalyzer(int idEntity)
            : this(Objects.ById<Entity>(idEntity))
        {
        }

        public EntityAnalyzer(Entity entity)
        {
            TargetEntity = entity;
            denormInfo = IoC.Resolve<DenornalizedEntitiesInfo>();

            setProperties();
            setAnalyzer();
        }

        #region Public Members

        /// <summary>
        /// Зарегистрирована ли сущность <paramref name="idEntity"/> как родительская денормализованная ТЧ ?
        /// </summary>
        public bool IsMasterTablepart { get; private set; }

        /// <summary>
        /// Зарегистрирована ли сущность <paramref name="idEntity"/> как дочерняя денормализованная ТЧ ?
        /// </summary>
        public bool IsChildTablepart { get; private set; }

        #endregion

        #region Private Members

        private void setProperties()
        {
            IsMasterTablepart = denormInfo.IsMasterTablepart(TargetEntity.Id);
            IsChildTablepart = denormInfo.IsChildTablepart(TargetEntity.Id);
        }

        private void setAnalyzer()
        {
            if (IsChildTablepart)
            {
                _tpAnalyzer = denormInfo.GetAnalyzerByChild(TargetEntity.Id);
            }
            else if (IsMasterTablepart)
            {
                _tpAnalyzer = denormInfo.GetAnalyzerByMaster(TargetEntity.Id);
            }
        }

        #endregion
    }
}
