using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class ToolAppResponse : AppResponse
    {
        #region Свойства, актуальные только для элемента документа

        /// <summary>
        /// Список операций для отображения в виде выпадающего меню
        /// </summary>
        public ResponseOperations Operations;

        /// <summary>
        /// Идентификатор выполняемой операции.
        /// Свойство имеет значение только в случае, если над открытым элементом документа начата операция. 
        /// Соответственно ее можно либо применить, либо отменить.
        /// </summary>
        public int? CurrentOperationId;

        /// <summary>
        /// Редактируемые поля.
        /// Если null - считаем, что все поля разрешены редактировать.
        /// Если коллекция пустая, то ни одно поле нельзя редактировать, элемент открыт только для чтения.
        /// </summary>
        public List<string> EditableFields;

        /// <summary>
        /// Информация о записях в регистрах
        /// выполненных документом
        /// </summary>
        public List<Registry.RecordsInfo> RegistryRecords;

        #endregion

    }
}
