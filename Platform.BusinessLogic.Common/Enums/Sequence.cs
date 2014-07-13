using System;

namespace Platform.BusinessLogic.Common.Enums
{
    /// <summary>
    /// Когда выполняется действие (до или после вставки, обновления или удаления)
    /// </summary>
    [Flags]
    public enum Sequence
    {
        /// <summary>
        /// до
        /// </summary>
        Before=1,
        /// <summary>
        /// после
        /// </summary>
        After =2 ,

        /// <summary>
        /// до и после
        /// </summary>
        Any = Before|After
    }
}
