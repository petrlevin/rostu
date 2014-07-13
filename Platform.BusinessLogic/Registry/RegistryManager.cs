using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Registry
{
    /// <summary>
    /// Информация о регистрах
    /// </summary>
    public class RegistryManager
    {
        private readonly DataContext _dbContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public RegistryManager([Dependency]DbContext dbContext)
        {
            _dbContext = dbContext.Cast<DataContext>();
        }

        /// <summary>
        /// Получение информации о движениях регистров для документа
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        public List<RecordsInfo> GetInfo(IEntity entity, int docId)
        {



            var regs = _dbContext.Entity.Where(e => (e.IdEntityType == (byte)EntityType.Registry) &&
                                                    (_dbContext.EntityField.Any(ef=>ef.IdEntity==e.Id&& ef.Name.ToLower() == "IdRegistrator".ToLower()))).ToList();
            int entityId = entity.Id;

            IQueryable<RI> query =null;
            IQueryable<RI> curQuery = null;
            foreach (var regEntity in regs)
            {
                
                if (
                    regEntity.Fields.Any(
                        ef =>
                        (ef.Name.ToLower() == "IdRegistrator".ToLower()) &&
                        ef.IsCommonReference()
                ))
                {
                    Entity re = regEntity;
                    curQuery =_dbContext.Set<IHasCommonRegistrator>(regEntity)
                                                   .Where(r=>(r.IdRegistrator ==docId)&& (r.IdRegistratorEntity==entityId))
                                                   .Select(r => new RI() {Caption = re.Caption, Id=re.Id});
                    
                    


                }
                else if (regEntity.Fields.Any(
                    ef =>
                    (ef.Name.ToLower() == "IdRegistrator".ToLower()) &&
                    ((ef.EntityFieldType == EntityFieldType.Link) ||
                     (ef.IdEntityLink == entityId))))

                {
                    Entity re = regEntity;
                    curQuery = _dbContext.Set<IHasRegistrator>(regEntity)
                                             .Where(r => (r.IdRegistrator == docId))
                                             .Select(r => new RI() { Caption = re.Caption, Id = re.Id });
                }
                else
                    continue;
                query = query == null
                                                ? curQuery
                                                : query.Concat(curQuery);



            }



            var q = query.GroupBy(hc => new  {hc.Caption , hc.Id})
                         .Select(g => new RecordsInfo() { Caption = g.Key.Caption, Id = g.Key.Id, Count = g.Count() });
            
            return q.ToList();


        }
    }

    internal class RI
    {
        public string Caption { get; set; }
        public int Id { get; set; }
    }

}
