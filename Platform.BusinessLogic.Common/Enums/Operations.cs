using Platform.Common;

namespace Platform.BusinessLogic.Common.Enums
{
    public enum Operations
    {
        [EnumCaption("Редактирование")] 
        Update =1,

        [EnumCaption("Удаление")] 
        Delete =2,

        [EnumCaption("Добавление")] 
        Insert =3
    }
}
