using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Factoring;

namespace Platform.PrimaryEntities.Tests.Mocks
{
	[ExcludeFromCodeCoverage]
	public class Factory : Factory<IDictionary<string, object>>
    {
        protected override bool IsParent<TChild, TParent>()
        {
            return true;
        }
    }
}
