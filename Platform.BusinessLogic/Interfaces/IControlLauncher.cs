using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.BusinessLogic.Interfaces
{
    /// <summary>
    /// лончер контролей
    /// </summary>
    public interface IControlLauncher
    {
        /// <summary>
        /// Выполнить контроли для объекта <paramref name="entityValue"/> на <paramref name="controlType"/> и <paramref name="sequence"/>
        /// например - выполнить все контроли перед вставкой
        /// </summary>
        /// <param name="controlType">тип контроля (вставка , обновление , удаление )</param>
        /// <param name="sequence">порядок следования (до или после операции вставки обновления удаления)</param>
        /// <param name="entityValue">объект прикладной сущности для которой выполняется контрол</param>
        /// <param name="oldEntityValue">старое значение объекта прикладной сущности </param>
        /// <exception cref="ControlResponseException"></exception>
        /// <exception cref="ControlExecutionException"></exception>
        /// <exception cref="ControlDefinitionException"></exception>
        void ProcessControls(ControlType controlType, Sequence sequence, IBaseEntity entityValue , IBaseEntity oldEntityValue);

        void InvokeControl(Action<IBaseEntity> control, IBaseEntity entity, MemberInfo memberInfo, IControlInfo initialControlInfo);
    }
}
