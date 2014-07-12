using Sbor.Reference;

namespace Sbor.Logic
{
    /// <summary>
    /// Тип элемента - тип родительского элемента
    /// </summary>
    public class SGModel
    {
        /// <summary>
        /// Тип элемента СЦ
        /// </summary>
        public ElementTypeSystemGoal ElementType;

        /// <summary>
        /// Тип родителя для элемента СЦ
        /// </summary>
        public ElementTypeSystemGoal ElementParentType;
    }
}
