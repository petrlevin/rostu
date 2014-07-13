using Platform.BusinessLogic.Common.Exceptions;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IControlInteraction
    {
        bool MaySkip(ControlResponseException ex);
    }
}
