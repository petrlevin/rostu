using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// Реализация исполнителя контролей
    /// </summary>
    public class ConcreteControlLauncher : ControlLauncher
    {
        private readonly IBaseEntity _baseEntity;

        /// <summary>
        /// Исполнитель, использующий диспетчером контролей по-умолчанию
        /// </summary>
        /// <param name="baseEntity"></param>
        /// <param name="dbContext"></param>
        /// <param name="controlDispatcher"></param>
        public ConcreteControlLauncher([Dependency("ControlTarget")]IBaseEntity baseEntity, [Dependency]DbContext dbContext, [Dependency]IControlDispatcher controlDispatcher) :base(dbContext , controlDispatcher)
        {
            _baseEntity = baseEntity;
        }

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
        public override void ProcessControls(ControlType controlType, Sequence sequence, IBaseEntity entityValue, IBaseEntity oldEntityValue)
        {

            if (!_baseEntity.Equals(entityValue))
                    return;
            base.ProcessControls(controlType, sequence, entityValue, oldEntityValue);
        }

    }
}
