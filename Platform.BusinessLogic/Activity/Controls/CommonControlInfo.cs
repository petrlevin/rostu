using System;
using System.Linq;
using System.Reflection;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// Информация об общем контроле
    /// </summary>
    public static class CommonControlInfo
    {
        /// <summary>
        /// Создает изначальную информацию об общем  контроле (полученную из кода а не из базы)
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        public static IControlInfo GetInitial(Type entityType, Type control)
        {
            var result = Cache.Get<IControlInfo>(entityType, control);
            if (result != null)
                return result;

            result = DoGet(entityType, control);
            Cache.Put(result,entityType,control);
            return result;
        }

        private static IControlInfo DoGet(Type entityType, Type control)
        {
            ControlInitialForAttribute atrrInCntrl;
            ControlInitialForAttribute atrrInEnt;

            ControlAttributeHelper.GetAttributes(control, entityType, "определены начальные значения", out atrrInCntrl,
                                                 out atrrInEnt);

            ControlInitialForAttribute attr = atrrInCntrl ?? atrrInEnt;
            var mainAttr = ControlAttributeHelper.GetAttributeExactlyMatch<ControlInitialAttribute>(control);
            return InitialControlInfo.Merge(mainAttr, attr, control, entityType);
        }

        private static ISimpleCache Cache = new SimpleCache();
    }
}
