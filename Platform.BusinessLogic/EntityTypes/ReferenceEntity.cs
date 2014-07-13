using Platform.BusinessLogic.EntityTypes.Interfaces;
using Platform.PrimaryEntities;

namespace Platform.BusinessLogic.EntityTypes
{
	/// <summary>
	/// Базовый класс для всех справочников
	/// </summary>
	public abstract class ReferenceEntity : BaseEntity, IReferenceEntity
	{
        public virtual int Id { get; set; }
	}
}
