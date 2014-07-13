using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.PrimaryEntities.Factoring
{
    public static class EntityFieldExtension
    {

        public static IEntityField OwnerField(this IEntityField tablePartField)
        {
            if (!tablePartField.IdOwnerField.HasValue)
                return null;
            return Objects.ById<EntityField>(tablePartField.IdOwnerField.Value);
        }
    }
}
