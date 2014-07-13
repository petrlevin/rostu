using System.Data.SqlClient;
using Platform.BusinessLogic.Auditing.Auditors.Abstract;
using Platform.BusinessLogic.Common.Enums;
using Platform.Common;
using Platform.Dal;
using Platform.Dal.Serialization;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Auditing.Auditors
{
    public class ReferenceAuditor : CompletableAuditor
    {
        public void OnInsert(IEntity entity, int elementId)
        {
            logger.Log(entity.Id, elementId, Operations.Insert, null, GetSerializeCommand(entity, elementId).ExecuteScalar().ToString());
        }

        public void OnDelete(IEntity entity, int[] elementIds)
        {
            var fact = GetSerializeCommandFactory(entity);
            var conn = IoC.Resolve<SqlConnection>("DbConnection");
            foreach (int elementId in elementIds)
            {
                var id = elementId;
                var xml = fact.CreateCommand(elementId, conn).ExecuteScalar().ToString();
                complete += (time) => logger.Log(entity.Id, id, Operations.Delete, xml, null);
            }
        }

        public void OnUpdate(IEntity entity, int elementId)
        {
            On(entity, elementId, Operations.Update);
        }

        public void OnUpdate(IEntity entity, int[] elementIds)
        {
            foreach(var elementId in elementIds)
                On(entity, elementId, Operations.Update);
        }

        private void On(IEntity entity, int elementId, Operations operation)
        {
            var command = GetSerializeCommand(entity, elementId);
            var xml = command.ExecuteScalar().ToString();
            complete += (time) => logger.Log(entity.Id, elementId, operation, xml, operation == Operations.Update ? command.ExecuteScalar().ToString() : null);
        }

        protected SqlCommand GetSerializeCommand(IEntity entity, int elementId)
        {
            return GetSerializeCommandFactory(entity).CreateCommand(elementId, IoC.Resolve<SqlConnection>("DbConnection"));
        }

        protected SerializationCommandFactory GetSerializeCommandFactory(IEntity entity)
        {
            return new SimpleSelectBuilder(Objects.ById<Entity>(entity.Id)).Build();
        }
    }
}
