using Platform.BusinessLogic.DataAccess;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Denormalizer.Crud
{
	public class DenormalizedDeleter : DenormalizedCrud
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idEntity">id сущности, из которой удаляются элементы (дочерняя денормализованная ТЧ)</param>
		public DenormalizedDeleter(int idEntity): base(idEntity)
		{
		}

		public bool DeleteItem(int[] itemIds)
		{
		    Check();
			masterDm = DataManagerFactory.Create(TargetEntity);
			return masterDm.DeleteItem(itemIds);
		}
	}
}
