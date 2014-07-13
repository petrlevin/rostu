using System;
using System.Linq;
using System.Collections.Generic;
using BgTasks.Core;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.Utils.Collections;

namespace BgTasks.Wcf
{
    public class BackgroundTask: IBackgroundTask, IDisposable
    {
        /*
         * -1744830429 idEntity TestDocument
         * -1744830436 id №777
         * -1744830433 idEntityOperation Edit
         */

        private Proxy proxy;

        public BackgroundTask()
        {
            proxy = new Proxy();
            proxy.Init();
        }

        public void ProcessOperation(OperationPhase phase, int entityId, int docId, int entityOperationId)
        {
            EntityOperation entityOperation = proxy.Context.EntityOperation.Single(eo => eo.Id == entityOperationId);
            bool isAtomic = !entityOperation.EditableFields.Any();

            CommunicationContext cc = new CommunicationContext();

            if (isAtomic)
            {
                proxy.ProcessOperation(opMan => opMan.Exec(cc, entityId, docId, entityOperationId));
            }
            else
            {
                switch (phase)
                {
                    case OperationPhase.Exec:
                        proxy.ProcessOperation(opMan => opMan.BeginOperation(cc, entityId, docId, entityOperationId));
                        proxy.ProcessOperation(opMan => opMan.CompleteOperation(cc, entityId, docId, new IgnoreCaseDictionary<object>()));
                        break;
                    case OperationPhase.Begin:
                        proxy.ProcessOperation(opMan => opMan.BeginOperation(cc, entityId, docId, entityOperationId));
                        break;
                    case OperationPhase.Complete:
                        proxy.ProcessOperation(opMan => opMan.CompleteOperation(cc, entityId, docId, new IgnoreCaseDictionary<object>()));
                        break;
                    case OperationPhase.Cancel:
                        proxy.ProcessOperation(opMan => opMan.CancelOperation(cc, entityId, docId));
                        break;
                }
            }
        }

        public void Dispose()
        {
            proxy.Dispose();
        }
    }
}
