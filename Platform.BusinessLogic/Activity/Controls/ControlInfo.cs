using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;

namespace Platform.BusinessLogic.Activity.Controls
{
    public static class ControlInfo
    {
        public static IControlInfo GetInitial(Type entityType, MemberInfo control)
        {
            var result = Cache.Get<IControlInfo>(entityType, control);
            if (result != null)
                return result;

            result = DoGet(entityType, control);
            Cache.Put(result, entityType, control);
            return result;
        }



        private static IControlInfo DoGet(Type entityType, MemberInfo control)
        {
            var attr = (ControlInitialAttribute)control.GetCustomAttributes(typeof(ControlInitialAttribute), true).FirstOrDefault();
            if (attr != null)
            {
                return new InitialControlInfo(attr, control)
                {
                    IdEntity = entityType.GetEntity().Id
                };
            }
            else
            {
                return new InitialControlInfo()
                {

                    Caption =
                        control.Name,
                    Skippable = false,
                    IdEntity = entityType.GetEntity().Id
                };

            }
        }

        private static ISimpleCache Cache = new SimpleCache();

    }
}
