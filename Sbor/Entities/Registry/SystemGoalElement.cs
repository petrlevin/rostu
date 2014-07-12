using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.PrimaryEntities.Common.Interfaces;
using Sbor.Interfaces;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;


namespace Sbor.Registry
{
    public partial class SystemGoalElement : ICommonRegister
    {
        public void Terminate(DataContext context, int idTerminator, int entityId, DateTime dateTerminate)
        {
            this.IdTerminator = idTerminator;
            this.IdTerminatorEntity = entityId;
            this.DateTerminate = dateTerminate;

            foreach (var rec in context.AttributeOfSystemGoalElement.Where(r => r.IdSystemGoalElement == this.Id))
            {
                rec.IdTerminator = idTerminator;
                rec.IdTerminatorEntity = entityId;
                rec.DateTerminate = dateTerminate;
            }

            foreach (var rec in context.GoalTarget.Where(r => r.IdSystemGoalElement == this.Id))
            {
                rec.IdTerminator = idTerminator;
                rec.IdTerminatorEntity = entityId;
                rec.DateTerminate = dateTerminate;
            }
           
        }

        /// <summary>
        /// Контроль "Проверка ссылочной целостности"
        /// </summary>
        [ControlInitial(InitialCaption = "Проверка ссылочной целостности", InitialSkippable = false, InitialUNK = "100101")]
        [Control(ControlType.Delete, Sequence.Before)]
        public void Control_100101(DataContext context)
        {
            var attrib =
                context.AttributeOfSystemGoalElement.Where(w =>
                    w.IdSystemGoalElement_Parent == this.Id 
                    && !(w.IdRegistrator == this.IdRegistrator && w.IdRegistratorEntity == this.IdRegistratorEntity)
                ).Select(s => new
                    {
                        Caption = s.SystemGoalElement.SystemGoal.Caption, 
                        s.RegistratorEntity, 
                        s.IdRegistrator
                    })
                .ToList();

            if (attrib.Any())
            {
                string errMsg =
                    "Действие не выполнено. У следующего элемента СЦ обнаружены нижестоящие элементы из других документов:<br>" +
                    context.SystemGoal.Single(s => s.Id == IdSystemGoal).Caption;

                foreach (var f in attrib)
                {
                    var doc = context.Set<IIdentitied>(f.RegistratorEntity).Single(s => s.Id == f.IdRegistrator).ToString();
                    errMsg = errMsg + string.Format("<br> - {0} ({1})", f.Caption, doc);
                }

                Controls.Throw(errMsg);
            }
        }
    }
}
