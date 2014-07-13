using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Reference;

namespace Platform.BusinessLogic.Activity.Operations
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityOperation"></param>
    /// <param name="document"></param>
    public delegate void OperationsHandler(EntityOperation entityOperation, ToolEntity document);

}
