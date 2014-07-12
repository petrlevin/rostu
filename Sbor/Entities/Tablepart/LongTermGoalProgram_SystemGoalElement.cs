using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Logic;
using Sbor.Reference;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Tablepart
{
    public partial class LongTermGoalProgram_SystemGoalElement : TablePartEntity, ITpSystemGoalElement 
	{
        private List<SystemGoal> lOnAdd;

        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.LongTermGoalProgram.SingleOrDefault(a => a.Id == IdOwner);

            if (SystemGoal == null)
                SystemGoal = context.SystemGoal.SingleOrDefault(a => a.Id == IdSystemGoal);
        }

        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 0)]
        public void AutoDeleteChildren(DataContext context)
        {
            var childs = context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdParent == Id).ToList();
            using (new ControlScope())
            {
                foreach (var s in childs)
                {
                    context.LongTermGoalProgram_SystemGoalElement.Remove(s);
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
                && (w.IdSBP == Owner.IdSBP || !w.IdSBP.HasValue)
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
                while (RootSg.IdParent.HasValue)
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
            var qTp = context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdOwner == Owner.Id).ToList();

            // создаем новые записи
            var qNewItems = lOnAdd.Where(w => !qTp.Any(a => a.IdSystemGoal == w.Id)).Distinct();
            foreach (var item in qNewItems.Where(r => r.Id != this.IdSystemGoal))
            {
                context.LongTermGoalProgram_SystemGoalElement.Add(new LongTermGoalProgram_SystemGoalElement()
                {
                    Owner = Owner,
                    SystemGoal = item,
                    FromAnotherDocumentSE = qSg1.Contains(item),
                    IsMainGoal = (MainSg == item) && (MainSg.IdDocType_CommitDoc == Owner.IdDocType || MainSg.IdDocType_ImplementDoc == Owner.IdDocType)
                });
            }

            this.IsMainGoal = (MainSg == this.SystemGoal) && (MainSg.IdDocType_CommitDoc == Owner.IdDocType || MainSg.IdDocType_ImplementDoc == Owner.IdDocType);
                //&& !context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdOwner == Owner.Id && w.Id != this.Id).Any();

            context.SaveChanges();

            // дл€ записей из нашего документа обновл€ем вычислемые хранимые пол€ (в т.ч. восстанавливаем иерархию), показатели, значени€ показателей
            int[] items = context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdOwner == Owner.Id).Select(s => s.Id).ToArray();
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


        /// <summary>   
        ///  онтроль "Ќаличие нижесто€щих элементов при удалении"
        /// </summary>         
        [ControlInitial(InitialUNK = "0230")]
        [Control(ControlType.Delete, Sequence.After, ExecutionOrder = 10)]
        public void Control_0230(DataContext context)
        {
            if (!FromAnotherDocumentSE)
            {
                InitMaps(context);

                var ids = Owner.AllVersionDocIds;

                var qsge = context.SystemGoalElement.Where(w =>
                    !w.IdTerminator.HasValue
                    && w.IdSystemGoal == IdSystemGoal
                    && (w.IdRegistratorEntity == Owner.EntityId && ids.Contains(w.IdRegistrator))
                );

                List<string> list = qsge.Join(
                    context.AttributeOfSystemGoalElement.Where(w =>
                        !w.IdTerminator.HasValue
                        && (w.IdRegistratorEntity != EntityId || !ids.Contains(w.IdRegistrator))
                        && w.IdSystemGoalElement_Parent.HasValue
                    ),
                    a => a.Id, b => b.IdSystemGoalElement_Parent,
                    (a, b) => b
                ).Select(s => " - " + s.SystemGoalElement_Parent.SystemGoal.Caption).ToList();

                if (list.Any())
                    Controls.Check(list, string.Format(
                        "—ледующий элемент —÷ невозможно удалить, так как у него обнаружены нижесто€щие элементы из других документов:<br>{0}<br>{{0}}",
                        SystemGoal.Caption
                    ));


                bool fail = qsge.Join(
                    context.TaskVolume.Where(w =>
                        !w.IdTerminator.HasValue
                        && (w.IdRegistratorEntity != EntityId || !ids.Contains(w.IdRegistrator))
                    ),
                    a => a.Id, b => b.IdSystemGoalElement,
                    (a, b) => b
                ).Any();

                if (fail)
                    Controls.Throw(string.Format(
                        "—ледующий элемент —÷ невозможно удалить, так он реализуетс€ в рамках других документов:<br> - {0}",
                        SystemGoal.Caption
                    ));
            }
        }

        /// <summary>   
        ///  онтроль "Ќаличие основной цели в документе"
        /// </summary>         
        [ControlInitial(InitialUNK = "0236")]
        [Control(ControlType.Delete, Sequence.After, ExecutionOrder = 20)]
        public void Control_0236(DataContext context)
        {
            InitMaps(context);

            if (IsMainGoal && ((Owner.HasMasterDoc == true && Owner.IdDocType == DocType.LongTermGoalProgram) || Owner.IdDocType == DocType.SubProgramDGP))
                Controls.Throw("«апрещено, удал€ть основную цель документа.");
        }

        #region Implementation of ITpSystemGoalElement

        public DateTime ? ParentDateStart { get { return Parent.DateStart; } }
        public DateTime ? ParentDateEnd { get { return Parent.DateEnd; } }
        public int ? ParentIdElementTypeSystemGoal { get { return Parent.IdElementTypeSystemGoal; } }
        public ElementTypeSystemGoal ParentElementTypeSystemGoal { get { return Parent.ElementTypeSystemGoal; } }

        #endregion
	}
}

