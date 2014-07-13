using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace BaseApp.DbEnums
{
    /// <summary>
    /// Роли ответственных лиц
    /// </summary>
    public enum RoleResponsiblePerson : byte
    {
		/// <summary>
		/// пусто
		/// </summary>
        [EnumCaption("Пусто")]
        Empty = 0,
        
		/// <summary>
		/// Главный бухгалтер
		/// </summary>
        [EnumCaption("Главный бухгалтер")]
        ChiefAccountant = 1,
        
		/// <summary>
        /// Руководитель
		/// </summary>
        [EnumCaption("Руководитель")]
        Head = 2
    }
}
