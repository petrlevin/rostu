using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Denormalizer.Crud
{
	/// <summary>
	/// Create, Update, Delete (CUD) & Defaults
	/// </summary>
	public abstract class DenormalizedCrud : EntityAnalyzer
	{
        protected DataManager masterDm { get; set; }

		public DenormalizedCrud(int idEntity) : base(idEntity)
		{

		}

        protected void Check()
        {
            if (!IsMasterTablepart)
            {
                throw new PlatformException("Сущность, указанная в конструкторе, не является родительской в денормализованной ТЧ.");
            }
        }
	}
}
