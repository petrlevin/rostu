using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Caching.Common
{
    public interface IManagedCache : ICache
    {
        void Clear();
        bool Enabled { get; set; }
    }

    
}
