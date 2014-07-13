using System;

namespace BaseApp.Common.Interfaces
{
    /// <summary>
    /// ППО для системных измерений
    /// </summary>
    public interface IPublicLegalFormation
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        Int32 Id { get; }

        /// <summary>
        /// Вышестоящий
        /// </summary>
        int? IdParent { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Уровень
        /// </summary>
        int IdBudgetLevel { get;  }
        
        /// <summary>
        /// Группа доступа
        /// </summary>
        int IdAccessGroup { get; }

        /// <summary>
        /// Код субъекта РФ
        /// </summary>
        string Subject { get; }
    }
}
