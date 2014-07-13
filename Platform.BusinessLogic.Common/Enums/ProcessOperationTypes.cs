using Platform.Common;

namespace Platform.BusinessLogic.Common.Enums
{
    public enum ProcessOperationTypes
    {
        [EnumCaption("Операции над документами")] 
        EntityOperation = 1,
        
        [EnumCaption("Выполнение контроля")] 
        Control = 2
    }
}
