using System;

namespace Platform.PrimaryEntities.Common
{
    /// <summary>
    /// Атрибут-метка для указания на перечисления, которые следует загрузить на клиента. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class ClientEnumAttribute: Attribute
    {
    }
}
