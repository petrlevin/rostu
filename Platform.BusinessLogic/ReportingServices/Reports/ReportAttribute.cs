using System;

namespace Platform.BusinessLogic.ReportingServices.Reports
{
    /// <summary>
    /// Аттрибут-метка для отчетов
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ReportAttribute: Attribute
    {
    }
}
