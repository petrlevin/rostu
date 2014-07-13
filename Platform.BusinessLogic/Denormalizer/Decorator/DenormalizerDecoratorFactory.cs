using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Denormalizer.Decorator
{
    public class DenormalizerDecoratorFactory : EntityAnalyzer
	{
		public DenormalizerDecoratorFactory(Entity target): base(target)
		{
		}

		public DenormalizerDecorator GetDecorator(IEnumerable<int> periods)
		{
            if (!IsMasterTablepart)
				return null;

			var decorator = new DenormalizerDecorator()
				{
                    TpAnalyzer = TpAnalyzer,
					DenormalizedPeriods = periods
				};
			return decorator;
		}
	}
}
