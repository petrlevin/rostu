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
    public partial class StateProgram_SystemGoalElement : TablePartEntity, ITpSystemGoalElement 
	{

        private List<SystemGoal> lOnAdd;

        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.StateProgram.SingleOrDefault(a => a.Id == IdOwner);

            if (SystemGoal == null)
                SystemGoal = context.SystemGoal.SingleOrDefault(a => a.Id == IdSystemGoal);
        }

        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 0)]
        public void AutoDeleteChildren(DataContext context)
        {
            var childs = context.StateProgram_SystemGoalElement.Where(w => w.IdParent == Id).ToList();
            using (new ControlScope())
            {
                foreach (var s in childs)
                {
                    context.StateProgram_SystemGoalElement.Remove(s);
                    context.SaveChanges();
                }
            }
        }

        [ControlInitial(InitialUNK = "0134", InitialCaption = "������� �������� ���� � ���������")]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 0)]
        public void Control_0134(DataContext context)
        {
            InitMaps(context);

            //���� ��������������.��� ��������� = ������������ ��, 
            if (Owner.IdDocType != DocType.SubProgramSP)
            {
                return;
            }
            
            //� �� ��������� �ֻ ��������� �������� ��������� � ��������� ��������� ����� � ��� ����������� �������. 
            if (!IsMainGoal)
            {
                return;
            }

            Controls.Throw("���������, ������� �������� ���� ���������.");
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            InitMaps(context);

            //FromAnotherDocumentSE = (Owner.IdDocType != SystemGoal.IdDocType_CommitDoc);
            //context.SaveChanges();

            // ������ ���� ��� ����� ���� � ����� ������
            var qSg0 = context.SystemGoal.Where(w =>
                w.IdPublicLegalFormation == Owner.IdPublicLegalFormation
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

            // ���� �������� ������� �� ����� ���������� ������������ ������������, ������ �������� ���������� � ���� ������
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

            // ������ �� ������������ ���������� ���� � ��������� ��� ���������� ��������
            var curSg = this.SystemGoal.Id;
            InOnAdd(qOnSelect, curSg);// ���������� � ��������

            // �������� ������ �� ��, � ��������� �� ����� ���������
            var qTp = context.StateProgram_SystemGoalElement.Where(w => w.IdOwner == Owner.Id).ToList();

            // ������� ����� ������
            var qNewItems = lOnAdd.Where(w => !qTp.Any(a => a.IdSystemGoal == w.Id)).Distinct().Distinct();
            foreach (var item in qNewItems.Where(r => r.Id != this.IdSystemGoal))
            {
                context.StateProgram_SystemGoalElement.Add(new StateProgram_SystemGoalElement()
                {
                    Owner = Owner,
                    SystemGoal = item,
                    FromAnotherDocumentSE = qSg1.Contains(item),
                    IsMainGoal = (MainSg == item) && (MainSg.IdDocType_CommitDoc == Owner.IdDocType || MainSg.IdDocType_ImplementDoc == Owner.IdDocType)
                });
            }

            this.IsMainGoal = (MainSg == this.SystemGoal) && (MainSg.IdDocType_CommitDoc == Owner.IdDocType || MainSg.IdDocType_ImplementDoc == Owner.IdDocType);
                //&& !context.StateProgram_SystemGoalElement.Where(w => w.IdOwner == Owner.Id && w.IsMainGoal && w.Id != this.Id).Any();

            context.SaveChanges();

            // ��� ������� �� ������ ��������� ��������� ���������� �������� ���� (� �.�. ��������������� ��������), ����������, �������� �����������
            int[] items = context.StateProgram_SystemGoalElement.Where(w => w.IdOwner == Owner.Id).Select(s => s.Id).ToArray();
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
        /// �������� "������� ����������� ��������� ��� ��������"
        /// </summary>         
        [Control(ControlType.Delete, Sequence.After, ExecutionOrder = 10)]
        public void Control_0102(DataContext context)
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
                        "��������� ������� �� ���������� �������, ��� ��� � ���� ���������� ����������� �������� �� ������ ����������:<br>{0}<br>{{0}}",
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

