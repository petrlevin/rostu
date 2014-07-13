using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Controls.InvocationsProviders
{
    class CommonControls :Dictionary<Type,List<Type>>
    {
        public void Add(Type @for, Type control)
        {
            if (!ContainsKey(@for))
                Add(@for,new List<Type>());
            this[@for].Add(control);
        }
    }
}
