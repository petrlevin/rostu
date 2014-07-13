using System.Collections.Generic;
using System.Linq;
using Platform.BusinessLogic.DataAccess;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Denormalizer.Crud
{
	public class DenormalizedDefaults : DenormalizedCrud
	{
		protected DataManager childDm { get; set; }

		public DenormalizedDefaults(int idEntity) : base(idEntity)
		{
		}

        public InitialItemState GetDefaults(Dictionary<string, object> clientDefaultValues = null)
		{
            Check();

            masterDm = DataManagerFactory.Create(TargetEntity);
            childDm = DataManagerFactory.Create(TpAnalyzer.ChildTp);

            var childDefaults = childDm.GetInitialState();
            var masterDefaults = masterDm.GetInitialState(clientDefaultValues);

			childDefaults.Defaults = childDefaults.Defaults.Concat(masterDefaults.Defaults).ToDictionary(o => o.Key, o => o.Value);
			return childDefaults;
		}
	}
}
