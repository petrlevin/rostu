using System.Collections.Generic;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.Common;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Denormalizer.ModelTransformers
{
	public class ModelAnalyzer : EntityAnalyzer
	{
        /// <summary>
        /// Признак того, что механиз денормализации отключен
        /// </summary>
        protected const bool IsDisabled = false;

        private DenornalizedEntitiesInfo denormInfo { get; set; }

		public ModelAnalyzer(Entity entity) : base(entity)
		{
            denormInfo = IoC.Resolve<DenornalizedEntitiesInfo>();
		}

		/// <summary>
		/// Исключает из набора полей табличные части, являющиеся дочерними по отношению к денормализуемым
		/// </summary>
		/// <param name="list"></param>
		public void RemoveChildTablefields(List<IDictionary<string, object>> list)
		{
			if (IsDisabled)
				return;

			var id = "id";
			for (int i = list.Count-1; i >= 0; i--)
			{
				var props = list[i];
                if (props.ContainsKey(id) && denormInfo.IsChildTablefield((int)props[id]))
				{
					list.RemoveAt(i);
				}
			}
		}
	}
}
