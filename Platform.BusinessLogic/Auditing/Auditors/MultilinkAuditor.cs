using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.Auditing.Auditors.Abstract;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Auditing.Auditors
{
    public class MultilinkAuditor : CompletableAuditor
    {
        public void OnInsert(IEntity multilinkEntity, int leftEntityId, int leftElementId, int[] rightElementIds)
        {
            On(multilinkEntity, leftEntityId, leftElementId, rightElementIds, MultilinkOperations.Insert);
        }

        public void OnDelete(IEntity multilinkEntity, int leftEntityId, int leftElementId, int[] rightElementIds)
        {
            On(multilinkEntity, leftEntityId, leftElementId, rightElementIds, MultilinkOperations.Delete);
        }

        private void On(IEntity multilinkEntity, int leftEntityId, int leftElementId, int[] rightElementIds, MultilinkOperations operation)
        {
            IEntity rightEntity = MultilinkHelper.GetRightMultilinkEntity(multilinkEntity, leftEntityId);
            bool leftIsFirst = multilinkEntity.Fields.First(f =>
                        f.EntityLink != null
                        && (f.IdEntityLink == leftEntityId || f.IdEntityLink == rightEntity.Id)
                    ).IdEntityLink == leftEntityId;
            foreach (int rightElementId in rightElementIds.ToList())
            {
                complete +=
                    (ms) =>
                    logger.Log(multilinkEntity.Id, leftIsFirst ? leftElementId : rightElementId,
                               leftIsFirst ? rightElementId : leftElementId, operation);
            }
        }
    }
}
