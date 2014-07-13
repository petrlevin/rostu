using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BgTasks.Wcf
{
    /// <summary>
    /// Фаза неатомарной операции (для атомарной операции данное перечисление смысла не имеет).
    /// Перечисление применяется, когда для неатомарной операции следует указать фазу, которую следует выполнить.
    /// Например, в справочнике "Выполняемые операции" при постановке задач в очередь можно указать фазу операции, котоую следует исполнить.
    /// </summary>
    [DataContract]
    public enum OperationPhase
    {
        /// <summary>
        /// Выполнение обеих стадий неатомарной операции: начало и применение.
        /// При этом значения для редактируемых полей данной операции не будут установлены.
        /// </summary>
        [EnumMember]
        Exec,

        /// <summary>
        /// Начать операцию.
        /// </summary>
        [EnumMember]
        Begin,
        
        /// <summary>
        /// Применить начатую операцию.
        /// </summary>
        [EnumMember]
        Complete,
        
        /// <summary>
        /// Отменить начатую операцию.
        /// </summary>
        [EnumMember]
        Cancel
    }
}
