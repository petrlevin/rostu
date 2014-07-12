using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.DbEnums;

namespace Sbor.CommonControls
{
	[ControlInitial(ExcludeFromSetup = false, InitialSkippable = false, InitialManaged = false, InitialCaption = "Удаление элемента справочника")]
	[Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 1)]
	class ReferenceCheckStatusBeforeDelete : ICommonControl<IHasRefStatus, DataContext>
	{
		#region Implementation of ICommonControl<in IHasRefStatus,in DataContext>

		public void Execute(DataContext dataContext, ControlType controlType, Sequence sequence, IHasRefStatus element, IHasRefStatus oldElement)
		{
			if (element.RefStatus != RefStatus.New)
				Controls.Throw("Удаление возможно только для элементов на статусе 'Новый'");
		}

		#endregion
	}
}
