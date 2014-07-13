using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using BaseApp.Activity.Operations;
using BaseApp.Common.Interfaces;
using BaseApp.Interfaces;
using BaseApp.Reference;
using BaseApp.Reference.Mappings;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.Utils;
using Platform.Utils.Extensions;

namespace BaseApp
{
    /// <summary>
    /// Дата-контекст
    /// </summary>
    public partial class DataContext
    {
        /// <summary>
        /// 
        /// </summary>
        public DbSet<Control_Exceptions> Control_Exceptions { get; set; }
		
        /// <summary>
		/// Лицензии
		/// </summary>
		public DbSet<License> Licenses { get; set; }

        partial void CustomOnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new Control_ExceptionsMap());
			modelBuilder.Configurations.Add(new LicenseMap());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            var objectContext = ((IObjectContextAdapter) this).ObjectContext;
            
            
            objectContext.DetectChanges();

            var modified = objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified);

            var users = modified.Where(ose => ose.Entity is User).Select(o=>(User)o.Entity);

            ProcessRegistries(objectContext);

           //DeleteExecutedOperations(objectContext);

            int result = base.SaveChanges();

            users.ToList().ForEach(
                u=>u.OnAfterUpdate()

                );
            return result;
        }

        private void DeleteExecutedOperations(ObjectContext objectContext)
        {
            foreach (ToolEntity doc in objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Deleted).Where(e=>e.Entity is ToolEntity).Select(e=>e.Entity))
            {
                var exop =
                    ExecutedOperation.Where(eo => eo.IdRegistrator == doc.Id && eo.IdRegistratorEntity == doc.EntityId)
                                     .ToList();
                exop.ForEach(eo=>ExecutedOperation.Remove(eo));
            }
        }

        private static void ProcessRegistries(ObjectContext objectContext)
        {
            foreach (IRegistryWithTermOperation registryWithTermOperation in objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified)
                .Where(ose => ose.Entity is IRegistryWithTermOperation && ose.Entity is IHasTerminator && (((IHasTerminator)ose.Entity).IdTerminator == null) && (!(ose.OriginalValues.GetValue(columnName: Reflection<IHasTerminator>.Property(ht => ht.IdTerminator).Name) is System.DBNull)))
                             .Select(oe => (IRegistryWithTermOperation)oe.Entity).ToList())
            {

                registryWithTermOperation.IdTerminateOperation = null;
            }
        


            var operationContext = OperationContext.Current;
            while (operationContext != null)
            {
                operationContext.WriteRegistries();
                operationContext = operationContext.Previous;
            }



        }


        static partial void OnStaticConstruct()
        {
            DbContextExtension.RegisterContextName(typeof(DataContext),"базового приложения");
        }


        /// <summary>
        /// Проверить возможность вставки контроля в справочник 
        /// </summary>
        /// <param name="objectStateManager"></param>
        /// <exception cref="ControlAccessException">Если пользователь не суперюзер</exception>
        public void CheckControlUpdating(ObjectStateManager objectStateManager)
        {
            if (IoC.Resolve<IUser>("CurrentUser").IsSuperUser())
                return;
            if (objectStateManager.GetObjectStateEntries(EntityState.Added).Any(ose => (ose.Entity is Control)))
                throw new ControlAccessException("Добавлять новые контроли в справочник может только суперюзер");
        }
    }
}
