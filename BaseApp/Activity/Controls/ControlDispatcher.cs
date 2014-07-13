using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Reference;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;
using NLog;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;


namespace BaseApp.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ControlDispatcher : Platform.BusinessLogic.Activity.Controls.ControlDispatcher
    {
        private readonly IManagedCache _cache;

        /// <summary>
        /// Получить информацию о контроле
        /// </summary>
        /// <param name="controlName">Наименование контроля (имя метода)</param>
        /// <param name="target">Сущность</param>
        /// <returns></returns>
        protected override IControlInfo GetControlInfo(string controlName, IBaseEntity target)
        {
            var controlInfo = base.GetControlInfo(controlName, target);
            var control = controlInfo as Control;
            if (control == null)
                return controlInfo;
            if (!control.Managed)
                return control;

            var sysDimensions = IoC.Resolve<SysDimensionsState>("CurentDimensions");
            var controlException = ReadControlException(control, sysDimensions);
            return controlException ?? control;
        }



        /// <summary>
        /// Диспетчер выполнения контролей
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="dbContext"></param>
        /// <param name="strategy"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ControlDispatcher([Dependency("Cache")]IManagedCache cache, [Dependency] DbContext dbContext = null,
                                 [Dependency] Platform.BusinessLogic.Activity.Controls.DispatcherStrategies.DefaultStrategy
                                     strategy = null) :
            base(dbContext, strategy)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            _cache = cache;
        }


        private IControlInfo GetControlException(IQueryable<Control_Exceptions> controlExceptions,
                                                 SysDimensionsState sysDimensions ,Control control)
        {
            return controlExceptions.Where(
                ce =>
                (ce.IdOwner == control.Id) &&
                (ce.IdPublicLegalFormation == sysDimensions.PublicLegalFormation.Id) &&
                ((ce.IdBudget == sysDimensions.Budget.Id) || (ce.IdBudget == null)))
                                    .OrderByDescending(ce => ce.IdBudget)
                                    .FirstOrDefault();
        }


        private IControlInfo ReadControlException(Control control, SysDimensionsState sysDimensions)
        {
            if (!_cache.Enabled)
            {
                return GetControlException(DataContext<DataContext>()
                                               .Control_Exceptions, sysDimensions, control);

            }
            ;

            var controlExceptions = control.IdEntity.HasValue
                               ? _cache.Get<List<Control_Exceptions>>("Control_Exceptions", control.IdEntity.Value)
                               : _cache.Get<List<Control_Exceptions>>("Control_Exceptions");
            if (controlExceptions == null)
            {
                _loger.Debug("Кэш исключений  контролей пуст");
                var all = DataContext<DataContext>()
                    .Control_Exceptions.Include(ce=>ce.Owner);
                controlExceptions = control.IdEntity.HasValue
                                        ? all.Where(ce => ce.Owner.IdEntity == control.IdEntity.Value).ToList()
                                        : all.Where(ce => ce.Owner.IdEntity == null).ToList();
                if (control.IdEntity.HasValue)
                    _cache.PutWithCommandText(String.Format(@"
                    SELECT 
                     [c].[id]
                    ,[c].[Enabled]
                    ,[c].[Skippable]
                    ,[c].[Name]
                    ,[c].[Caption]
                    ,[c].[UNK]
                    ,[c].[Managed]
                    ,[ce].[idPublicLegalFormation]
                    ,[ce].[idBudget]
                    ,[ce].[Enabled]
                    ,[ce].[Skippable]

                    FROM [ref].[Control] [c] INNER JOIN [ref].[Control_Exceptions] [ce] ON ([c].[id]=[ce].[idOwner])  WHERE [c].[idEntity] = {0}" ,control.IdEntity.Value),

                    controlExceptions, "Control_Exceptions", control.IdEntity.Value);
                else
                    _cache.PutWithCommandText(@"
                    SELECT 
                     [c].[id]
                    ,[c].[Enabled]
                    ,[c].[Skippable]
                    ,[c].[Name]
                    ,[c].[Caption]
                    ,[c].[UNK]
                    ,[c].[Managed]
                    ,[ce].[idPublicLegalFormation]
                    ,[ce].[idBudget]
                    ,[ce].[Enabled]
                    ,[ce].[Skippable]

                    FROM [ref].[Control] [c] INNER JOIN [ref].[Control_Exceptions] [ce] ON ([c].[id]=[ce].[idOwner])  WHERE [c].[idEntity] IS NULL" ,

                    controlExceptions, "Control_Exceptions");


            }
            return  GetControlException(controlExceptions.AsQueryable(), sysDimensions, control);
        }

        /// <summary>
        /// Получить информацию о контроле. Если используется кэш - при первом обращении сохраняем результаты в кэш.
        /// </summary>
        /// <param name="controlName">Наименование контроля (имя метода)</param>
        /// <param name="target">Сущность</param>
        /// <returns></returns>
        protected override IControlInfo ReadControlInfo(string controlName, IBaseEntity target)
        {
            if (!_cache.Enabled)
                return base.ReadControlInfo(controlName, target);
            _loger.Debug(controlName);
            var controls = _cache.Get<List<Control>>("Controls", target.EntityId);
            
            if (controls == null)
            {
                _loger.Debug("Кэш пуст");
                controls = DataContext<DataContext>().Control.Where(c => (target.EntityId == c.IdEntity)).ToList();
                _cache.PutWithCommandText(String.Format(@"
                    SELECT 
                     [id]
                    ,[Enabled]
                    ,[Skippable]
                    ,[Name]
                    ,[Caption]
                    ,[UNK]
                    ,[Managed]
                    FROM [ref].[Control] WHERE [idEntity] = {0}",
                                               target.EntityId

                    ), controls,"Controls", target.EntityId);

            }
            var result = controls.FirstOrDefault(c => c.Name == controlName);

            if (result != null)
                return result;

            controls = _cache.Get<List<Control>>("CommonControls");
            if (controls == null)
            {
                _loger.Debug("Кэш общих контролей пуст");
                controls = DataContext<DataContext>().Control.Where(c => (null == c.IdEntity)).ToList();
                _cache.PutWithCommandText(@"
                    SELECT 
                     [id]
                    ,[Enabled]
                    ,[Skippable]
                    ,[Name]
                    ,[Caption]
                    ,[UNK]
                    ,[Managed]
                    FROM [ref].[Control] WHERE [idEntity] IS NULL"


                                          , controls, "CommonControls");

            }
            _loger.Debug("----------------------");
            return controls.FirstOrDefault(c => c.Name == controlName);
        }

        private static Logger _loger = LogManager.GetCurrentClassLogger();
    }
}
