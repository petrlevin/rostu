using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;

namespace Platform.BusinessLogic.Interfaces
{
    /// <summary>
    /// Справочники у которых есть статус
    /// должны потдерживатьэтот интерфейс
    /// </summary>
    public interface IHasRefStatus : IIdentitied
    {
        /// <summary>
        /// Статус справочника
        /// </summary>
        RefStatus RefStatus { get; set; }

        byte IdRefStatus { get; set; }
    }
}
