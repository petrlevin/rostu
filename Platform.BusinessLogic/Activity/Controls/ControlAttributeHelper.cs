using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// Работа с атрибутами контролей
    /// </summary>
    public static class ControlAttributeHelper
    {

        /// <summary>
        /// Получить аттрибут контроля
        /// </summary>
        /// <param name="control"></param>
        /// <typeparam name="TAtribute"></typeparam>
        /// <returns></returns>
        /// <exception cref="ControlDefinitionException"></exception>
        public static TAtribute GetAttributeExactlyMatch<TAtribute>(MemberInfo control)
            where TAtribute : Attribute
        {
            try
            {
                return control.GetAttributeExactlyMatch<TAtribute>();
            }
            catch (InvalidOperationException ex)
            {
                throw new ControlDefinitionException(String.Format(
                    "Контроль не правильно определен. К общему контролу типа '{0}'  применен аттрибут '{1}'   более чем один раз.",
                    control, typeof(TAtribute).Name
                                                         ), ex, control);

            }
            
        }

        /// <summary>
        /// Получить информацию о контроле из атрибутов метод или из типа сущности
        /// </summary>
        /// <param name="control"></param>
        /// <param name="entityType"></param>
        /// <param name="errorMessage"></param>
        /// <param name="atrrInCntrl">Информация из атрибута метода</param>
        /// <param name="atrrInEnt">Информация из атрибута класса сущности</param>
        /// <typeparam name="TAtribute"></typeparam>
        /// <exception cref="ControlDefinitionException"></exception>
        public static void GetAttributes<TAtribute>(MemberInfo control, Type entityType, string errorMessage, out TAtribute atrrInCntrl, out TAtribute atrrInEnt) where TAtribute : Attribute , IHasTarget
        {
            //определен порядок выполнения
            try
            {
                atrrInCntrl =
                    (TAtribute)control.GetCustomAttributes(typeof(TAtribute), true)
                                                 .SingleOrDefault(
                                                     a => ((TAtribute)a).Target == entityType);

            }
            catch (InvalidOperationException ex)
            {
                throw new ControlDefinitionException(String.Format(
                    "Контроль не правильно определен. К общему контролу типа '{0}'  применен аттрибут '{1}' с одинаковым типом сущности (Target) '{2}'  более чем один раз.",
                    control, typeof(TAtribute).Name, entityType
                                                         ), ex, control);

            }
            try
            {
                atrrInEnt =
                    (TAtribute)entityType.GetCustomAttributes(typeof(TAtribute), true)
                                                         .SingleOrDefault(
                                                             a => ((TAtribute)a).Target == control);

            }
            catch (InvalidOperationException ex)
            {
                throw new ControlDefinitionException(String.Format(
                    "Контроль не правильно определен. К сущностному классу типа '{0}'  применен аттрибут '{1}' с одинаковым типом общего контрола  (Target) '{2}'  более чем один раз.",
                    entityType, typeof(TAtribute).Name, control
                                                         ), ex, control);

            }

            if ((atrrInCntrl != null) && (atrrInEnt != null))
                throw new ControlDefinitionException(String.Format(
                    "Для общего контроля  '{0}'   {1} с помощью аттрибута '{2}' с одинаковым типом сущности (Type) '{3}'  и в определении контрола и в определении сущности. Неоднозначность.",
                    control, errorMessage, typeof(TAtribute).Name, entityType
                                                         ), control);
        }

    }
}
