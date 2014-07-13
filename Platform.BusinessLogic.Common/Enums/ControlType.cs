using System;

namespace Platform.BusinessLogic.Common.Enums
{
    /// <summary>
    /// Какая операция производится над объектом (вставка , удаление , обновление)  
    /// 
    /// </summary>
    [Flags]
    public enum ControlType
    {
        /// <summary>
        /// на вставку нового элемента
        /// </summary>
        Insert=1,
        /// <summary>
        /// обновление нового элемента
        /// </summary>
        Update=2,
        /// <summary>
        /// удаление элемента
        /// </summary>
        Delete=4 ,


        /// <summary>
        /// любая опперация
        /// </summary>
        Any = Insert|Update|Delete
    }
}
