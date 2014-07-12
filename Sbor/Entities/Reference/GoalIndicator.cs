using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Reference
{
    public partial class GoalIndicator : ReferenceEntity 
	{
        private void InitMaps(DataContext context)
        {
            if (SBP == null && IdSBP.HasValue)
                SBP = context.SBP.SingleOrDefault(a => a.Id == IdSBP);
        }

        [Control(ControlType.Insert, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            var ppo = context.PublicLegalFormation.SingleOrDefault(w => w.Id == IdPublicLegalFormation);
            if (ppo.IdMethodofFormingCode_TargetIndicator == (byte)BaseApp.DbEnums.MethodofFormingCode.Auto || string.Equals(Code, "јвтоматически"))
            {
                int maxNum = 0;

                var sc =
                    context.GoalIndicator.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation)
                           .Select(s => s.Code).Distinct();

                foreach (string s in sc)
                {
                    int i;
                    if (int.TryParse(s, out i))
                    {
                        maxNum = (maxNum < i ? i : maxNum);
                    }
                }

                Code = (maxNum + 1).ToString();
            }
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_501001(DataContext context)
        {
            InitMaps(context);

            GoalIndicator obj = context.GoalIndicator.FirstOrDefault(a =>
				a.IdPublicLegalFormation == IdPublicLegalFormation
				&& (a.IdSBP ?? 0) == (IdSBP ?? 0)
                && a.Caption == Caption
                && a.IdRefStatus != (byte)RefStats.Archive
                && a.Id != Id
            );
            if (obj != null)
                Controls.Throw(string.Format(
                    "¬ справочнике уже имеетс€ целевой показатель с таким же наименованием:<br> {0} - {1}{2}.",
                    obj.Code,
                    obj.Caption,
                    SBP != null ? " - " + obj.SBP.Caption : ""
                ));
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void Control_501002(DataContext context)
        {
            InitMaps(context);

            GoalIndicator obj = context.GoalIndicator.FirstOrDefault(a =>
				a.IdPublicLegalFormation == IdPublicLegalFormation
                && a.Code == Code
                && a.IdRefStatus != (byte)RefStats.Archive
                && a.Id != Id
            );
            if (obj != null)
                Controls.Throw(string.Format(
                    "¬ справочнике уже имеетс€ целевой показатель с таким же кодом:<br> {0} - {1}{2}.",
                    obj.Code,
                    obj.Caption,
                    SBP != null ? " - " + obj.SBP.Caption : ""
                ));
        }
    }
}

