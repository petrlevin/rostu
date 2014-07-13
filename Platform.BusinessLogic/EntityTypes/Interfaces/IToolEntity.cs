using System.Linq;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Interfaces
{
	/// <summary>
	/// Инструмент
	/// </summary>
	public interface IToolEntity :IBaseEntity ,IIdentitied
	{
        IQueryable<EntityOperation> GetAvailableOperations();

        int IdDocStatus { get; set; }

		DocStatus DocStatus { get; set; }
	}
}
