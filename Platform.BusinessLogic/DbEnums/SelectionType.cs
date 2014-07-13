using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common;

namespace Platform.BusinessLogic.DbEnums
{
    /// <summary>
    /// Тип выборки
    /// </summary>
    [ClientEnum]
    public enum SelectionType
    {
        
        /// <summary>
        /// Список сущностей
        /// </summary>
        Entities =1,
       
        /// <summary>
        /// Запрос на получение списка сущностей
        /// </summary>
        EntitiesSql =2 ,

        /// <summary>
        /// Запрос на получение списка пар сущность , элемент
        /// </summary>
        EntitiesItemsSql = 3 ,

        /// <summary>
        /// Всё
        /// </summary>
        All =4

    }
}
