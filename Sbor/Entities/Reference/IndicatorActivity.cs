using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Reference
{

    public partial class IndicatorActivity : ReferenceEntity 
	{

        ///// <summary>   �������� ������� �� ������
        ///// �������� "�������� ������������ ������������ ����������"
        ///// </summary>
        //[Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 1)]
        //public void Control_501801(DataContext context)
        //{
        //    var sfMsgErr = "� ����������� ��� ������� ���������� � ����� �� �������������:<br>{0}{1}";

        //    var exists = context.IndicatorActivity.Where(r =>
        //        r.IdPublicLegalFormation == IdPublicLegalFormation &&
        //        r.Caption.Trim() == Caption.Trim() &&
        //        ( (!r.IdSBP.HasValue && !this.IdSBP.HasValue) || r.SBP == SBP ) &&
        //        r.IdRefStatus != (byte)RefStats.Archive &&
        //        r.Id != Id); 
           
        //    if (exists.Any())
        //    {
        //        var e = exists.FirstOrDefault();
        //        Controls.Throw(string.Format(sfMsgErr, e.Caption, (e.IdSBP.HasValue ? " - " + e.SBP.Caption : "")));
        //    }
        //}
    }
}

