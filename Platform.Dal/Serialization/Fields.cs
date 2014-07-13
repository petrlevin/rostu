using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Orderable;

namespace Platform.Dal.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public static class Fields
    {
        static public List<IEntityField> For(IEntity entity, Options options = Options.Default)
        {
            if (options == Options.TableParts)
                return entity.Fields.Where(ef => ef.EntityFieldType == EntityFieldType.Tablepart).ToList();
            if (options == Options.TablePartsMasterFirst)
                return GetTablePartsMasterFirst(entity);
            if (options == Options.Multilink)
                return entity.Fields.Where(ef => ef.EntityFieldType == EntityFieldType.Multilink).ToList();



            var fields = entity.RealFields.Where(f => !(f.IdCalculatedFieldType.HasValue && f.IsDbComputed));


            if (options == Options.WithoutId)
                fields = fields.Where(f => f.Name != "id");
            return fields.ToList();
        }

        private static List<IEntityField> GetTablePartsMasterFirst(IEntity entity)
        {
            var tps = entity.Fields.Where(ef => ef.EntityFieldType == EntityFieldType.Tablepart).ToList();
            if (tps.Count == 0)
                return tps;
            var first = Item<IEntityField>.FromList(tps, null, (f, s) =>
                    {
                        var tp = f.EntityLink;
                        return
                            Fields.For(tp)
                                  .Where(
                                      ef =>
                                      (ef.EntityFieldType == EntityFieldType.Link) &&
                                      (ef.EntityLink.EntityType == EntityType.Tablepart))
                                  .Any(ef => ef.IdEntityLink == s.IdEntityLink);

                    }
                );

            bool wasMoovings = false;
            var current = first;
            do
            {
                if (current == null)
                {
                    wasMoovings = false;
                    current = first.First();
                }

                wasMoovings = current.MoveAfterByPredicate() || wasMoovings;
                current = current.Next;
                
            } while (wasMoovings);

            return first.First().ToList();
        }
    }

    public enum Options
    {
        Default = 1,
        WithoutId = 2,
        TableParts = 3,
        TablePartsMasterFirst = 4,
        Multilink = 5
    }
}
