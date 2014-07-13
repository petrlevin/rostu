using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic
{
    /// <summary>
    /// Идентикаторы сущностей для  сущностных классов
    /// </summary>
    public static class EntityIds
    {
        private static readonly Dictionary<Type, Int32> Entries = new Dictionary<Type, int>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entityId"></param>
        static public void Add(Type type, Int32 entityId)
        {
            Entries.Add(type, entityId);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Int32? Get(Type type)
        {
            if (Entries.ContainsKey(type))
                return Entries[type];
            else
                return null;
        }


    }

}
