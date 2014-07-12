using Platform.Common;

namespace Sbor.Reports.DbEnums
{
    /// <summary>
    /// Тип выводимых программ
    /// </summary>
    public enum ListTypeOutputProgram
    {
        /// <summary>
        /// Государственная Программа
        /// </summary>
        [EnumCaption("Государственная Программа")]
        StateProgram = 1,

        /// <summary>
        /// Подпрограмма ГП
        /// </summary>
        [EnumCaption("Подпрограмма ГП")]
        SubProgramState = 2,

        /// <summary>
        /// Ведомственная целевая программа
        /// </summary>
        [EnumCaption("Ведомственная целевая программа")]
        DepartmentalTargetProgram = 3,

        /// <summary>
        /// Основное Мероприятие
        /// </summary>
        [EnumCaption("Основное Мероприятие")]
        MainEvent = 4,

        /// <summary>
        /// ВЦП ; ОМ
        /// </summary>
        [EnumCaption("ВЦП ; ОМ")]
        MainEventDepartmentalTargetProgram = 5,

        
        /// <summary>
        /// Гос. Программа ; Подпрограмма ГП
        /// </summary>
        [EnumCaption("Гос. Программа ; Подпрограмма ГП")]
        StateProgramSubProgramState = 6,


        /// <summary>
        /// Гос. Программа ; Подпрограмма ГП ; ВЦП ; ОМ
        /// </summary>
        [EnumCaption("Гос. Программа ; Подпрограмма ГП ; ВЦП ; ОМ")]
        StProgSubStProgMainEventDepartTargProg = 7
    }
}
