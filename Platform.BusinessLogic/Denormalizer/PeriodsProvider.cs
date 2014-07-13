using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.Denormalizer.ModelTransformers;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Denormalizer
{
	public class PeriodsProvider : PeriodsProviderBase
	{
	    private DenornalizedEntitiesInfo info;
        private DenormalizedTablepartAnalyzer tpAnalyzer;

		/// <summary>
		/// Сущность-владелец денормализованной ТЧ
		/// </summary>
		private Entity entity;
		private IBaseEntity entityItem;
		private IColumnFactoryForDenormalizedTablepart entityItemColumnFactory;
        
		#region Public Getters

        private ColumnsInfo _columns;
        /// <summary>
		/// Информация о значимых колонах ДТЧ
		/// </summary>
        public ColumnsInfo Columns
		{
			get
			{
				if (_columns == null)
					_columns = entityItemColumnFactory.GetColumns(tpAnalyzer.MasterTp.Name);
				return _columns;
			}
		}

		/// <summary>
		/// Идентификаторы периодов, для каждого из которых будет создан набор значимых колонок
		/// </summary>
		public IEnumerable<int> PeriodIds
		{
			get { return Columns.Periods.Select(c => c.PeriodId); }
		}

        /// <summary>
        /// Поля значимых колонок
        /// </summary>
	    public IEnumerable<IEntityField> PeriodFields
	    {
	        get
	        {
                return tpAnalyzer.GetConfiguredValueFields(entityItem.EntityId, Columns);
	        }
	    }

		/// <summary>
		/// Модель значимых полей
		/// </summary>
		public IEnumerable<IDictionary<string, object>> ColumnsModel
		{
			get
			{
				var model = new List<IDictionary<string, object>>();

                foreach (IEntityField entityField in PeriodFields)
                {
                    model.Add(toDictionary(entityField));
                }

				return model;
			}
		}

		#endregion

        /// <summary>
        /// Поставщик информации о колонках денормализованной ТЧ
        /// </summary>
        /// <param name="entityId">id сущности-владельца</param>
        /// <param name="itemId">id элемента сущности-владельца</param>
        /// <param name="tpEntityId">Идентификатор родительской сущности денормализованной ТЧ</param>
		public PeriodsProvider(int entityId, int itemId, int tpEntityId)
		{
            info = IoC.Resolve<DenornalizedEntitiesInfo>();
			this.entity = Objects.ById<Entity>(entityId);

            if (!info.IsMasterTablepart(tpEntityId))
            {
                throw new PlatformException(string.Format("Сущность с id = ТЧ {0}, находящаяся в сущности с id = {1}, не зарегистрирована среди денормализованных", tpEntityId, entityId));
            }

            this.tpAnalyzer = info.GetAnalyzerByMaster(tpEntityId);

            if (!entity.GenerateEntityClass)
            {
                throw new PlatformException(
				    "Денормализованная сущность, для которой предпринята попытка получить значимые колонки периодов находится в сущносте, не имеющего сущностного класса (определено по признаку GenerateEntityClass=false)." +
				    "Значимые колонки периодов создаются на основе массива идентификаторов периодов, для получения которого сущность должна иметь сущностной класс," +
				    "реализующий интерфейс IColumnFactoryForDenormalizedTablepart."
				    );
            }

			var entityManager = new EntityManager(entity);
			entityItem = entityManager.Find(itemId);
			
            if (!(entityItem is IColumnFactoryForDenormalizedTablepart))
			{
				throw new PlatformException("Сущностной класс не реализует интерфейса IColumnFactoryForDenormalizedTablepart");    
			}

            // => все в порядке
			this.entityItemColumnFactory = (IColumnFactoryForDenormalizedTablepart)entityItem;
		}

	}
}
