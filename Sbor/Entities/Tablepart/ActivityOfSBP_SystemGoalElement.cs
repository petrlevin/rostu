using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Tablepart
{
    public partial class ActivityOfSBP_SystemGoalElement : TablePartEntity, ITpSystemGoalElement
    {
        private List<SystemGoal> lOnAdd;

        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.ActivityOfSBP.SingleOrDefault(a => a.Id == IdOwner);

            if (SystemGoal == null)
                SystemGoal = context.SystemGoal.SingleOrDefault(a => a.Id == IdSystemGoal);
        }

        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 1)]
        public void AutoDeleteChildren(DataContext context)
        {
            var childs = context.ActivityOfSBP_SystemGoalElement.Where(w => w.IdParent == Id).ToList();
            using (new ControlScope())
            {
                foreach (var s in childs)
                {
                    context.ActivityOfSBP_SystemGoalElement.Remove(s);
                    context.SaveChanges();
                }
            }
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            InitMaps(context);

            //FromAnotherDocumentSE = (Owner.IdDocType != SystemGoal.IdDocType_CommitDoc);
            //context.SaveChanges();

            // список того что могло быть в форме выбора
            var qSg0 = context.SystemGoal.Where(w =>
                w.IdPublicLegalFormation == Owner.IdPublicLegalFormation
                && w.IdSBP == Owner.IdSBP
                && (w.IdDocType_CommitDoc == Owner.IdDocType || w.IdDocType_ImplementDoc == Owner.IdDocType)
                && w.DateStart >= Owner.DateStart && w.DateEnd <= Owner.DateEnd
                && w.IdRefStatus == (byte)RefStats.Work
            ).ToList();

            var qSg1 =
                qSg0.Where(w => w.IdParent.HasValue)
                    .Where(w => w.Parent.IdDocType_CommitDoc != Owner.IdDocType && w.Parent.IdRefStatus == (byte)RefStats.Work).Select(w => w.Parent).ToList();

            var qOnSelect = qSg0.Concat(qSg1);

            lOnAdd = new List<SystemGoal>();

            SystemGoal MainSg = this.SystemGoal;

            // ищем корневой Ёлемент —÷ среди допустимых относительно добавл€емого, походу добавл€€ пройденные в спец список
            var RootSg = this.SystemGoal;
            if (RootSg.IdParent.HasValue)
            {
                while ( RootSg.IdParent.HasValue )
                {
                    if (qOnSelect.Any(w => w.Id == RootSg.IdParent))
                    {
                        RootSg = RootSg.Parent;
                        if (RootSg.Id != this.SystemGoal.Id)
                        {
                            lOnAdd.Add(RootSg);
                            if (MainSg != RootSg && (RootSg.IdDocType_CommitDoc == Owner.IdDocType || RootSg.IdDocType_ImplementDoc == Owner.IdDocType))
                            {
                                MainSg = RootSg;
                            }
                        }
                        if (RootSg.IdDocType_CommitDoc != Owner.IdDocType)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // теперь от добавл€емого спускаемс€ вниз и добавл€ем все допустимые элементы
            var curSg = this.SystemGoal.Id;
            InOnAdd(qOnSelect, curSg);// добавление в рекурсии

            // получаем записи из тч, с признаком из этого документа
            var qTp = context.ActivityOfSBP_SystemGoalElement.Where(w => w.IdOwner == Owner.Id).ToList();

            // создаем новые записи
            var qNewItems = lOnAdd.Where(w => !qTp.Any(a => a.IdSystemGoal == w.Id)).Distinct();

            foreach (var item in qNewItems.Where(r => r.Id != this.IdSystemGoal))
            {
                context.ActivityOfSBP_SystemGoalElement.Add(new ActivityOfSBP_SystemGoalElement()
                {
                    Owner = Owner,
                    SystemGoal = item,
                    FromAnotherDocumentSE = qSg1.Contains(item),
                    IsMainGoal = (MainSg == item) && (MainSg.IdDocType_CommitDoc == Owner.IdDocType || MainSg.IdDocType_ImplementDoc == Owner.IdDocType)
                });
            }

            this.IsMainGoal = (MainSg == this.SystemGoal) && (MainSg.IdDocType_CommitDoc == Owner.IdDocType || MainSg.IdDocType_ImplementDoc == Owner.IdDocType);
                //&& !context.ActivityOfSBP_SystemGoalElement.Where(w => w.IdOwner == Owner.Id && w.IsMainGoal && w.Id != this.Id).Any();

            context.SaveChanges();

            // дл€ записей из нашего документа обновл€ем вычислемые хранимые пол€ (в т.ч. восстанавливаем иерархию), показатели, значени€ показателей
            int[] items = context.ActivityOfSBP_SystemGoalElement.Where(w => w.IdOwner == Owner.Id).Select(s => s.Id).ToArray();
            Owner.RefreshData_SystemGoalElement(context, items);
            Owner.FillData_GoalIndicator_Value(context, items);


        }

        private void InOnAdd(IEnumerable<SystemGoal> qOnSelect, int curSg)
        {
            var childs = qOnSelect.Where(w => w.IdParent == curSg && (w.IdDocType_CommitDoc == Owner.IdDocType || w.IdDocType_ImplementDoc == Owner.IdDocType));
            foreach (var child in childs)
            {
                lOnAdd.Add(child);
                InOnAdd(qOnSelect, child.Id);
            }
        }

        [ControlInitial(InitialCaption = "”даление основной цели документа, вход€щего в состав √ѕ.", InitialUNK = "0333")]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 0)]
        public void Control_0333(DataContext context)
        {
            InitMaps(context);

            if (!IsMainGoal)
            {
                return;
            }

            if (!Owner.HasMasterDoc)
            {
                return;
            }

            Controls.Throw("«апрещено, удал€ть основную цель документа, вход€щего в состав √ѕ.");
        }

        /// <summary>   
        ///  онтроль "Ќаличие нижесто€щих элементов при удалении"
        /// </summary>         
        [Control(ControlType.Delete, Sequence.After, ExecutionOrder = 10)]
        public void Control_0326(DataContext context)
        {
            if (!FromAnotherDocumentSE)
            {
                InitMaps(context);

                var ids = Owner.AllVersionDocIds;

                List<string> list = context.SystemGoalElement.Where(w =>
                    !w.IdTerminator.HasValue
                    && w.IdSystemGoal == IdSystemGoal
                    && (w.IdRegistratorEntity == Owner.EntityId && ids.Contains(w.IdRegistrator))
                ).Join(
                    context.AttributeOfSystemGoalElement.Where(w =>
                        !w.IdTerminator.HasValue
                        && (w.IdRegistratorEntity != EntityId || !ids.Contains(w.IdRegistrator))
                        && w.IdSystemGoalElement_Parent.HasValue
                    ),
                    a => a.IdSystemGoal, b => b.SystemGoalElement_Parent.IdSystemGoal,
                    (a, b) => b
                ).Select(s => " - " + s.SystemGoalElement.SystemGoal.Caption).ToList();

                if (list.Any())
                    Controls.Check(list, string.Format(
                        "—ледующий элемент —÷ невозможно удалить, так как у него обнаружены нижесто€щие элементы из других документов:<br>{0}<br>{{0}}",
                        SystemGoal.Caption
                    ));
            }
        }

        #region Implementation of ITpSystemGoalElement

        public DateTime ? ParentDateStart { get { return Parent.DateStart; } }
        public DateTime ? ParentDateEnd { get { return Parent.DateEnd; } }
        public int ? ParentIdElementTypeSystemGoal { get { return Parent.IdElementTypeSystemGoal; } }
        public ElementTypeSystemGoal ParentElementTypeSystemGoal { get { return Parent.ElementTypeSystemGoal; } }

        #endregion
	}
}

