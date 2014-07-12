using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using System.Linq;

namespace Sbor.Tablepart
{
    public partial class DocumentsOfSED_ItemsSystemGoal
	{
        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.DocumentsOfSED.SingleOrDefault(a => a.Id == IdOwner);

            if (SystemGoal == null)
                SystemGoal = context.SystemGoal.SingleOrDefault(a => a.Id == IdSystemGoal);
        }

        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 0)]
        public void AutoDeleteChildren(DataContext context)
        {
            var childs = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdParent == Id).ToList();
            using (new ControlScope())
            {
                foreach (var s in childs)
                {
                    context.DocumentsOfSED_ItemsSystemGoal.Remove(s);
                    context.SaveChanges();
                }
            }
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            InitMaps(context);

            IsOtherDocSG = (Owner.IdDocType != SystemGoal.IdDocType_CommitDoc);
            context.SaveChanges();

            List<int> items = new List<int>() { Id };

            if (!IsOtherDocSG)
            {
                int? cursg = IdSystemGoal;

                do
                {
                    var next =
                        context.SystemGoal.Where(w =>
                            w.Id == cursg
                            && w.IdParent.HasValue
                            && w.Parent.IdRefStatus == (byte)RefStatus.Work
                            && w.Parent.DateStart >= Owner.DateStart && w.Parent.DateEnd <= Owner.DateEnd
                            && !context.DocumentsOfSED_ItemsSystemGoal.Any(k => k.IdOwner == IdOwner && k.IdSystemGoal == w.IdParent)
                        )
                        .Select(s => s.Parent)
                        .SingleOrDefault();

                    if (next == null) break;

                    var isOtherDocSG = (Owner.IdDocType != next.IdDocType_CommitDoc);

                    var newItem = new DocumentsOfSED_ItemsSystemGoal()
                    {
                        IdOwner = IdOwner, 
                        IdSystemGoal = next.Id,
                        IsOtherDocSG = isOtherDocSG
                    };
                    context.DocumentsOfSED_ItemsSystemGoal.Add(newItem);
                    context.SaveChanges();
                    items.Add(newItem.Id);

                    if (isOtherDocSG || !next.IdParent.HasValue) break;

                    cursg = next.Id;

                } while (true);
            }

            var itms = items.ToArray();

            Owner.RefreshData_ItemsSystemGoals(context, itms);

            if (IsOtherDocSG)
            {
                var qGi = context.DocumentsOfSED_GoalIndicator.Where(w => w.IdMaster == Id);
                foreach (var item in qGi)
                {
                    context.DocumentsOfSED_GoalIndicator.Remove(item);
                }
            }
            else
            {
                Owner.FillData_GoalIndicatorValues(context, itms);
            }
        }

        /// <summary>   
        /// Контроль "Наличие нижестоящих элементов при удалении"
        /// </summary>         
        [Control(ControlType.Delete, Sequence.After, ExecutionOrder = 10)]
        public void Control_0018(DataContext context)
        {
            if (!IsOtherDocSG)
            {
                InitMaps(context);

                var ids = Owner.GetIdAllVersionDoc(context);

                List<string> list = context.SystemGoalElement.Where(w =>
                    !w.IdTerminator.HasValue
                    && w.IdSystemGoal == IdSystemGoal
                    && (w.IdRegistratorEntity == Owner.EntityId && ids.Contains(w.IdRegistrator))
                ).Join(
                    context.AttributeOfSystemGoalElement.Where(w =>
                        !w.IdTerminator.HasValue
                        && (w.IdRegistratorEntity != Owner.EntityId || !ids.Contains(w.IdRegistrator))
                        && w.IdSystemGoalElement_Parent.HasValue
                    ),
                    a => a.Id, b => b.IdSystemGoalElement_Parent,
                    (a, b) => b
                ).Select(s => " - " + s.SystemGoalElement.SystemGoal.Caption).Distinct().ToList();

                if (list.Any())
                    Controls.Check(list, string.Format(
                        "Следующий элемент СЦ невозможно удалить, так как у него обнаружены нижестоящие элементы из других документов:<br>{0}<br>{{0}}",
                        SystemGoal.Caption
                    ));
            }
        }

    }
}

