using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.Denormalizer.Analyzers
{
	public class DenornalizedEntitiesInfo
	{
        public class Registrator : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                RegisterIn(unityContainer);
            }
        }

		public static void RegisterIn(IUnityContainer unityContainer)
		{
			var temp = new DenornalizedEntitiesInfo();
            temp.Init();

            unityContainer.RegisterInstance(typeof(DenornalizedEntitiesInfo), temp);
		}

        /// <summary>
        /// Информация о денормализованных ТЧ
        /// </summary>
        private Dictionary<int, DenormalizedTablepartAnalyzer> childTableparts;

        #region Public Members

        public void Init()
		{
            DataContext db = IoC.Resolve<DbContext>("AppStartContext").Cast<DataContext>();
			//using (DataContext db = new DataContext())
			//{
                addDenormalized(db);
			//}
		}

        /// <summary>
        /// Возвращает анализатор денормализованной ТЧ по дочерней сущности с id <paramref name="idEntity"/>
        /// </summary>
        /// <param name="idEntity"></param>
        /// <returns></returns>
        public DenormalizedTablepartAnalyzer GetAnalyzerByChild(int idEntity)
        {
            return childTableparts[idEntity];
        }

        /// <summary>
        /// Возвращает анализатор денормализованной ТЧ по родительской сущности с id <paramref name="idEntity"/>
        /// </summary>
        /// <param name="idEntity"></param>
        /// <returns></returns>
        public DenormalizedTablepartAnalyzer GetAnalyzerByMaster(int idEntity)
        {
            return childTableparts.Select(kvp => kvp.Value).Single(tpa => tpa.MasterTp.Id == idEntity);
        }

        /// <summary>
        /// Зарегистрирована ли сущность <paramref name="idEntity"/> как родительская денормализованная ТЧ ?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
	    public bool IsMasterTablepart(int idEntity)
		{
            return childTableparts.Select(kvp => kvp.Value).Any(tpa => tpa.MasterTp.Id == idEntity);
		}

        /// <summary>
        /// Зарегистрирована ли сущность <paramref name="idEntity"/> как дочерняя денормализованная ТЧ ?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsChildTablepart(int idEntity)
        {
            return childTableparts.ContainsKey(idEntity);
        }

        /// <summary>
        /// Является ли поле <paramref name="fieldId"/> табличным полем дочерей ТЧ, по отношению к денормализованной ТЧ.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
		public bool IsChildTablefield(int fieldId)
		{
            return childTableparts.Select(kvp => kvp.Value).Any(tpa => tpa.ChildTablefield.Id == fieldId);
		}

        #endregion

        #region Private Methods

        private void addDenormalized(DataContext db)
	    {
            childTableparts = new Dictionary<int, DenormalizedTablepartAnalyzer>();
	        List<string> denormalized = getTypes<IChildDenormalized>().ToList();
            foreach (string tpName in denormalized)
            {
                Entity tpEntity = db.Entity.SingleOrDefault(e => e.Name == tpName & e.IdEntityType == (byte) EntityType.Tablepart);
                if (tpEntity != null)
                {
                    var analyzer = new DenormalizedTablepartAnalyzer(tpEntity, db);
                    analyzer.StuctureShouldBeCorrect();
                    childTableparts.Add(tpEntity.Id, analyzer);
                }
            }
	    }

		private IEnumerable<Assembly> getAppAssemblies()
		{
			var appAssemblies = new string[]
				{
					"Platform.BusinessLogic",
                    "BaseApp",
					"Sbor",
					"Tests"
				};

			return AppDomain.CurrentDomain.GetAssemblies().Where(a => appAssemblies.Contains(a.GetName().Name));
		}

        /// <summary>
        /// Имена классов, реализующих интерфейс <see cref="TInterface"/>
        /// </summary>
        /// <returns></returns>
		private IEnumerable<string> getTypes<TInterface>()
		{
			IEnumerable<Assembly> assemblies = getAppAssemblies();
            var interfaceType = typeof(TInterface);
			
			return assemblies.SelectMany(a => a.GetTypes())
				.Where(interfaceType.IsAssignableFrom)
				.Select(t => t.Name);
        }

        #endregion
        
    }
}
