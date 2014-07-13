using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp
{
    public static class SysDimensionHelper
    {
        private static int[] _sysDimensionTypeIds;
        /// <summary>
        /// Идентификаторы сущностей-сис.измерений
        /// </summary>
        public static int[] SysDimensionTypeIds
        {
            get
            {
                if (_sysDimensionTypeIds == null)
                {
                    List<int> ids = new List<int>();
                    foreach (var dimension in Enum.GetValues(typeof(SysDimension)))
                    {
                        var entityName = dimension.ToString();
                        var entity = Objects.ByName<Entity>(entityName);
                        ids.Add(entity.Id);
                    }
                    _sysDimensionTypeIds = ids.ToArray();
                }
                
                
                return _sysDimensionTypeIds;
            }
        }

        /// <summary>
        /// Возвращаем тип сущности для элемента системного измерения
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Type GetTypeForSysDimension(SysDimension element)
        {
            return GetTypeForSysDimension(element.ToString());
        }

        /// <summary>
        /// Возвращаем тип сущности для элемента системного измерения
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public static Type GetTypeForSysDimension(string entityName)
        {
            return Type.GetType(String.Format("BaseApp.Reference.{0}", entityName ));
        }
    
    }
}
