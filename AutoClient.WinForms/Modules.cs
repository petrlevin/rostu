using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoClient.Helpers;

namespace AutoClient.Scripts
{
    /// <summary>
    /// 
    /// </summary>
    public class Modules
    {
        /// <summary>
        /// Просто какие-то сущности для добавления в мультиссылку. 23 шт.
        /// </summary>
        List<int> entityIds = new List<int>
            {
                -2147483615,
                -2147483614,
                -2147483613,
                -2147483612,
                -2147483611,
                -2147483610,
                -2147483609,
                -2147483608,
                -2147483603,
                -2147483602,
                -2147483499,
                -2147483498,
                -2147483497,
                -2147483496,
                -2147483495,
                -2147483494,
                -2147483491,
                -2147483490,
                -2147483488,
                -2147483487,
                -2147483486,
                -2147483485,
                -2147483483,
            };

        private Redirector redirector;

        public Modules(Redirector redirector)
        {
            this.redirector = redirector;
        }

        public void AddRange()
        {
            var ml = getMultilinkHelper();
            Sequencer.Do(ml, (multilink, itemId) => multilink.Add(itemId), entityIds);
        }

        public void DeleteRange()
        {
            var ml = getMultilinkHelper();
            Sequencer.Do(ml, (multilink, itemId) => multilink.Delete(itemId), entityIds);
        }

        private MultilinkHelper getMultilinkHelper()
        {
            return new MultilinkHelper()
            {
                Redirector = redirector,
                OwnerEntityId = -1543503795, // Модули
                EntityId = -1543503793, // Сущность мультиссылки Модули-Сущности
                IdEntityItem = -1610612709 // Базовый модуль
            };
        }

    }
}
