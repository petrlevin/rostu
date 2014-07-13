using System;

namespace Platform.BusinessLogic.Common.Attributes
{
    /// <summary>
    /// Помечаем аттрибутом сущность, с которой происходит выбор иерархичского справочника
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SelectionWithNoChildsAttribute : Attribute

    {
      
    }

    /// <summary>
    /// Помечаем этим аттрибутом иерархический справочник, к которому может применяться правило выбора только листовых элементов
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SelectedWithNoChildsAttribute : Attribute
    {

    }
}
