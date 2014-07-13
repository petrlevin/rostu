using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHierarhy : IIdentitied
    {
        /// <summary>
        /// вышестоящий
        /// </summary>
		int? IdParent { get; }
    }
}
