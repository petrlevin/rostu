using System.Collections.Generic;

namespace Platform.Dal.Interfaces
{
    public interface IHasRequirements
    {

        IEnumerable<IRequirement> GetRequirements();
    }
}
