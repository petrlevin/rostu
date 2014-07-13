using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// Определяет начальное состояние контроля (любого: cud, общего, свободного, общего-свободного), т.е.
    /// тот набор свойств, которым будет обладать контроль при записи в справочник "Контроли".
    /// Начальное состояние контроля считывается при выполнении задачи "setcontrols" утилиты MH (setcontrols включена в deploydb).
    /// Если при выполнении задачи setcontrols в справочнике "Контроли" отсутствует запись о данном контроле, 
    /// то она будет создана с параметрами, указанными в свойствах с префиксом Initial данного атрибута.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    public class ControlInitialAttribute : Attribute
    {
        /// <summary>
        /// Исключить из установки контролей.
        /// Признак, влияющий на генерацию элемента справочника "Контроли" при выполнении задачи (task) "setcudcontrols" MigrationHelper'ом.
        /// Если признак установлен (true), то при выполнении задачи setcudcontrols элемент справочника не будет создан.
        /// </summary>
        public virtual bool ExcludeFromSetup { get; set; }

        /// <summary>
        /// Начальное состояние признака "Мягкий".
        /// </summary>
        public virtual bool InitialSkippable { get; set; }
        
        /// <summary>
        /// Начальное состояние признака "Управляемый".
        /// </summary>
        public virtual bool InitialManaged { get; set; }

        /// <summary>
        /// Начальное состояние поля "Код УНК" (Уникальный номер контроля).
        /// </summary>
        public string InitialUNK { get; set; }

        /// <summary>
        /// Русское наименование контроля. Начальное состояние.
        /// </summary>
        public string InitialCaption { get; set; }
    }
}
