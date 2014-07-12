using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.PrimaryEntities.Common.Interfaces
{
    /// <summary>
    /// Документ c с полями ППО и Версия
    /// </summary>
    public interface IPpoVerDoc : IHierarhy
    {
        /// <summary>
        /// ППО
        /// </summary>
        int IdPublicLegalFormation { get; set; }

        /// <summary>
        /// Версия
        /// </summary>
        int IdVersion { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        int IdDocType { get; set; }
    }
}
