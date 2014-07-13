using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.NavigationPanel
{
    /// <summary>
    /// Поставщик наименований сущностей для панели навигации.
    /// См. единственный публичный метод <see cref="Replace"/>.
    /// </summary>
    public class CaptionsProvider
    {
        /// <summary>
        /// Для каждой сущности из переданного в метод списка производится проверка:
        /// 1. Если для данной сущности в справочнике "Настройки сущностей" указано значение в поле "Класс для выбора наименования", то
        /// будет создан экземпляр данного класса и вызван его метод GetItemName интерфейса <see cref="INavigationPanelItemCaptionSelector"/>.
        /// </summary>
        /// <param name="rows">
        /// Список элементов сущности Entity. 
        /// Каждый элемент представляет собой словарь: ключ - имя поля, значение.
        /// </param>
        public static void Replace(List<IDictionary<string, object>> rows)
        {
            
        }
    }
}
