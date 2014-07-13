using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace Platform.PrimaryEntities.Factoring
{
    public class DefaultFactory : Factory
    {
        [Dependency("FactoryStrategy")]
        public override Interfaces.IFactoryStrategy<DataRow> FactoryStrategy
        {
            get
            {
                return base.FactoryStrategy;
            }
            set
            {
                base.FactoryStrategy = value;
            }
        }

    }
}
