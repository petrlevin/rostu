using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Extensions
{
    public static class MemberInfoExtension
    {
        public static TAtribute GetAttributeExactlyMatch<TAtribute>(this MemberInfo memberInfo)
            where TAtribute : Attribute
        {
            try
            {
                return
                    (TAtribute)memberInfo.GetCustomAttributes(typeof(TAtribute), true)
                                                 .SingleOrDefault(
                                                     a => ((TAtribute)a).GetType() == typeof(TAtribute));

            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(String.Format("Для '{0}' атрибут '{1}' применен более чем один раз.", memberInfo,typeof(TAtribute)),ex);

            }

        }

    }
}
