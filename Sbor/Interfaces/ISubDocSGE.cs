using Sbor.Logic;

namespace Sbor.Interfaces
{
    public interface ISubDocSGE: IDocSGE
    {
        /// <summary>
        /// Документ-создатель
        /// </summary>
        int? IdMasterDoc { get; set; }

		/// <summary>
		/// Ответственный исполнитель
		/// </summary>
		int IdSBP { get; set; }

		/// <summary>
		/// Тип ответственного исполнителя
		/// </summary>
		int IdResponsibleExecutantType { get; set; }

		/// <summary>
		/// Код
		/// </summary>
		int? IdAnalyticalCodeStateProgramValue { get; set; }

        /// <summary>
        /// обновить записи тч
        /// </summary>
        void RefreshData_SystemGoalElement(DataContext context, int[] items, bool flag = false);

        /// <summary>
        /// обновить связанные показатели
        /// </summary>
        void FillData_GoalIndicator_Value(DataContext context, int[] items);

        /// <summary>
        /// Операция изменить
        /// </summary>
        void Change(DataContext context);
    }
}

