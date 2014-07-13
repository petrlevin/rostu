using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.AppServices
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ListFormActionAttribute : Attribute
    {
        /// <summary>
        /// Наименование сущности
        /// </summary>
        public string EntityName { get; set; }
        
        /// <summary>
        /// Имя пункта в меню "Действия"
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Порядковый номер пункта меню
        /// </summary>
        public int Order { get; set; }
    }
}
