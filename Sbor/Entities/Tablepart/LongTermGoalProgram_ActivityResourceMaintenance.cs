using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Document;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    public partial class LongTermGoalProgram_ActivityResourceMaintenance : ITpResourceMaintenance
	{
        // !!! �� ������� !!!
        // !!! ������ �������� ��������� �������� ���� �� ����� �������� ������ ��������� ��� ���
        /// <summary>   
        /// �������� "�������� ������� ���������� ����������� ��� �  �� �� ���� ������ � ������� �� � ��� ������� �� �� "
        /// </summary> 
        //[ControlInitial(InitialCaption = "�������� ������� ���������� ����������� ��� �  �� �� ���� ������ � ������� �� � ��� ������� �� �� ", InitialUNK = "0130")]
        //[Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 50)]
        //public void Control_0130(DataContext context)
        //{
        //    var tpResourceMaintenance0 = context.LongTermGoalProgram_ActivityResourceMaintenance.Where(r => r.IdOwner == this.IdOwner && r.IdMaster == this.IdMaster).ToList();
        //    var tpResourceMaintenance_Value0 = context.LongTermGoalProgram_ActivityResourceMaintenance_Value.Where(r => r.IdOwner == this.IdOwner).ToList();

        //    LongTermGoalProgram.CtrlPart0226(tpResourceMaintenance0, tpResourceMaintenance_Value0);
        //}
    }
}