using Sbor.Logic;


namespace Sbor.Tablepart
{
	/// <summary>
	/// Ресурсное обеспечение подпрограмм
	/// </summary>
    public partial class StateProgram_SubProgramResourceMaintenance : ITpResourceMaintenance
	{
        // !!! НЕ УДАЛЯТЬ !!!
        // !!! Данный контроль веременно отключен пока не будут налажены вызовы контролей для ДТЧ
        // /// <summary>   
        // /// Контроль "Проверка наличия ресурсного обеспечения подпрограмм за один период в разрезе ИФ и без разреза по ИФ "
        // ///</summary> 
        //[ControlInitial(InitialCaption = "Проверка наличия ресурсного обеспечения подпрограмм за один период в разрезе ИФ и без разреза по ИФ ", InitialUNK = "0129")]
        //[Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 50)]
        //public void Control_0129(DataContext context)
        //{
        //    var tpResourceMaintenance0 = context.StateProgram_SubProgramResourceMaintenance.Where(r => r.IdOwner == this.IdOwner && r.IdMaster == this.IdMaster).ToList();
        //    var tpResourceMaintenance_Value0 = context.StateProgram_SubProgramResourceMaintenance_Value.Where(r => r.IdOwner == this.IdOwner).ToList();

        //    StateProgram.CtrlPart0129(tpResourceMaintenance0, tpResourceMaintenance_Value0);
        //}
	}
}