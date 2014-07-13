using System.Collections.Generic;
using System.Linq;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic
{
    /// <summary>
    /// 
    /// </summary>
    static public class EntityOperationExtension
    {
        /// <summary>
        /// Для всех операций кроме операции "редактировать" возвращает ее редактируемые поля.
        /// Для операции "редактировать" ("edit") возвращает ее редактируемые поля если они указаны,
        /// а если не указаны (список пуст) все поля документа или инструмента
        /// </summary>
        /// <param name="entityOperation"></param>
        /// <returns></returns>
        public static IEnumerable<IEntityField> ActualEditableFields(this EntityOperation entityOperation)
        {
            if (entityOperation.EditableFields.Any())
                return entityOperation.EditableFields;
            if (entityOperation.Operation.Name.ToLower() == "edit")
                return entityOperation.Entity.Fields;
            return entityOperation.EditableFields;
        }
    }
}
