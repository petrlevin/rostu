using System;

namespace Platform.BusinessLogic.ReportingServices.PrintForms
{
    /// <summary>
    /// Атрибут для поментки классов печатных форм
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PrintFormAttribute: Attribute
    {
        /// <summary>
        /// Русское наименование печатной формы.
        /// Отображается в сплитбаттоне документа.
        /// </summary>
        public string Caption { get; set; }
    }
}
