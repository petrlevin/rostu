using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Interfaces;
using BaseApp.Registry;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils;
using Platform.Utils.Extensions;

namespace BaseApp.Activity.Operations
{
    /// <summary>
    /// Контекст выполнения операции
    /// </summary>
    public class OperationContext : Stacked<OperationContext>
    {
        #region NonPublic
        private readonly List<IRegistryWithOperation> _notOfMineRegistries;
        private readonly List<IRegistryWithTermOperation> _notOfMineTermRegistries;
        private readonly ObjectContext _objectContext;
        
        private List<IRegistryWithOperation> GetRegistries()
        {
            _objectContext.DetectChanges();
            return _objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added)
                             .Where(ose => ose.Entity is IRegistryWithOperation)
                             .Select(oe => (IRegistryWithOperation)oe.Entity).ToList();
        }

        private List<IRegistryWithTermOperation> GetTermRegistries()
        {
            _objectContext.DetectChanges();

            return _objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified)
                .Where(ose => ose.Entity is IRegistryWithTermOperation && ose.Entity is IHasTerminator && (((IHasTerminator)ose.Entity).IdTerminator != null) && (ose.OriginalValues.GetValue(columnName: Reflection<IHasTerminator>.Property(ht => ht.IdTerminator).Name) is System.DBNull))
                             .Select(oe => (IRegistryWithTermOperation)oe.Entity).ToList();
        }




        /// <summary>
        /// Перед завершением операции записываем информацию в регистры (выполненные операции и информацию о прекращениях в регистрах)
        /// </summary>
        protected override void BeforeDispose()
        {
            WriteRegistries();
        }

        internal void WriteRegistries()
        {
            foreach (IRegistryWithOperation registryWithOperation in GetRegistries().Where(r => !_notOfMineRegistries.Contains(r)))
            {
                if (registryWithOperation.ExecutedOperation == null)
                    registryWithOperation.ExecutedOperation = ExecutedOperation;
            }

            foreach (IRegistryWithTermOperation registryWithOperation in GetTermRegistries().Where(r => !_notOfMineTermRegistries.Contains(r)))
            {
                if (registryWithOperation.TerminateOperation == null)
                    registryWithOperation.TerminateOperation = ExecutedOperation;
            }

        }


        #endregion

        /// <summary>
        /// Состояние объекта, на момент начала операции
        /// </summary>
        public IBaseEntity OriginalTarget { get; private set; }

        /// <summary>
        /// Создаем контекст выполнения операции с указанием выполненных операций
        /// </summary>
        /// <param name="executedOperation"></param>
        public OperationContext(ExecutedOperation executedOperation)
        {
            var dbContext = IoC.Resolve<DbContext>().Cast<DataContext>();
            _objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
            _notOfMineRegistries = GetRegistries();
            _notOfMineTermRegistries = GetTermRegistries();


            ExecutedOperation = executedOperation;
            OriginalTarget = OperationWideContext.Current!=null?OperationWideContext.Current.OriginalTarget:null;
        }

        /// <summary>
        /// 
        /// </summary>
        public ExecutedOperation ExecutedOperation { get; private set; }


    }
}
