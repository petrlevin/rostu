using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.EntityFramework;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.GeneratedCode.Reference.Mappings;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Reference.Mappings;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils;

namespace Platform.BusinessLogic
{
    public partial class DataContext
    {
        public DbSet<Entity> Entity { get; set; }
        public DbSet<EntityField> EntityField { get; set; }
        public DbSet<FormElement> FormElement { get; set; }
        public DbSet<InterfaceControl> InterfaceControl { get; set; }
        public DbSet<Filter> Filter { get; set; }
        public DbSet<Control> Control { get; set; }
        public DbSet<Form> Form { get; set; }

        partial void CustomOnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EntityFieldMap());
            modelBuilder.Configurations.Add(new EntityMap());
            modelBuilder.Configurations.Add(new FormElementMap());
            modelBuilder.Configurations.Add(new InterfaceControlMap());
            modelBuilder.Configurations.Add(new FilterMap());
            modelBuilder.Configurations.Add(new ControlMap());
            modelBuilder.Configurations.Add(new FormMap());
            modelBuilder.Ignore<FormedEntity>();
        }




        private int DoSaveChanges()
        {
            using (var saveChangeContext = new SaveChangeContext())
            {
                return DoSaveChanges(saveChangeContext);
            }
        }


        private int DoSaveChanges(SaveChangeContext saveChangeContext)
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                SqlExceptionHandler sqlExceptionHandler = new SqlExceptionHandler(exception);
                var newException = sqlExceptionHandler.Process();
                if ((!(newException is TransactionDeadlockedException)) ||
                    (saveChangeContext.AttemptCount >= TransactionDeadlocked.GetAttempsCount()))
                    throw newException;
                saveChangeContext.AttemptCount++;
                return DoSaveChanges(saveChangeContext);
            }
            
        }


        readonly Stack _controlStack = new Stack();

        public override int SaveChanges()
        {
            
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            objectContext.DetectChanges();
            var entries = objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified |
                                                                   EntityState.Deleted).ToList();
            IControlLauncher controlLauncher = null;
            try
            {
                controlLauncher = ControlLauncher.Current;
            }
            catch (Exception)
            {
                return DoSaveChanges();
            }
            if (controlLauncher is Striker)
                return DoSaveChanges();
            
            _controlStack.Push(entries.Select(e => e.Entity).ToList());
            try
            {
                return  DoSaveChanges(entries.Select(e=> new EntityInfo()
                                                              {
                                                                  Value =(IBaseEntity) e.Entity,
                                                                  OldValue = ((e.State == EntityState.Modified) || (e.State == EntityState.Deleted)) ? (IBaseEntity)Entry(e.Entity).OriginalValues.ToObject() : null,
                                                                  State = e.State

                                                              }).ToList(), controlLauncher);
            }
            finally
            {
                _controlStack.Pop();

            }
        }

        private int DoSaveChanges(IEnumerable<EntityInfo> entities, IControlLauncher controlLauncher)
        {

            foreach (var entity in entities)
            {
                if (_controlStack.Contains(entity.Value))
                    continue;
                if (entity.State == EntityState.Added)
                    controlLauncher.ProcessControls(ControlType.Insert, Sequence.Before, entity.Value, null);
                if (entity.State == EntityState.Modified)
                    controlLauncher.ProcessControls(ControlType.Update, Sequence.Before, entity.Value,entity.OldValue
                                                    );
                if (entity.State == EntityState.Deleted)
                    controlLauncher.ProcessControls(ControlType.Delete, Sequence.Before, entity.OldValue, null);
            }
            var result = DoSaveChanges();
            foreach (var entity in entities)
            {
                if (_controlStack.Contains(entity.Value))
                    continue;
                if (entity.State == EntityState.Added)
                    controlLauncher.ProcessControls(ControlType.Insert, Sequence.After, entity.Value, null);
                if (entity.State == EntityState.Modified)
                    controlLauncher.ProcessControls(ControlType.Update, Sequence.After, entity.Value,
                                                    entity.OldValue);
                if (entity.State == EntityState.Deleted)
                    controlLauncher.ProcessControls(ControlType.Delete, Sequence.After, entity.Value, null);
            }
            DoSaveChanges();
            return result;
        }



        static partial void OnStaticConstruct()
        {
            DbContextExtension.RegisterContextName(typeof(DataContext), "бизнес логики");
        }


        class Comparer : IEqualityComparer<IBaseEntity>
        {
            public bool Equals(IBaseEntity x, IBaseEntity y)
            {
                if ((!(x is IIdentitied)) || (!(y is IIdentitied)))
                    return ReferenceEquals(x, y);
                if (x.GetType() != y.GetType())
                    return false;
                return ((IIdentitied)x).Id == ((IIdentitied)y).Id;

            }

            public int GetHashCode(IBaseEntity obj)
            {
                    if (!(obj is IIdentitied))
                        return base.GetHashCode();
                    return ((IIdentitied)obj).Id.GetHashCode();

            }
        }


        

        class EntityInfo
        {
            public IBaseEntity Value { get; set; }
            public IBaseEntity OldValue { get; set; }
            public EntityState State { get; set; } 
        }

        class Stack
        {
            private readonly List<List<object>> _changed = new List<List<object>>();
            private List<object> _last;

            public void Push(List<Object> list)
            {
                if (_last != null)
                    _changed.Add(_last);
                _last = list;
            }

            public void Pop()
            {
                if (_changed.Count > 0)
                {
                    _last = _changed.Last();
                    _changed.RemoveAt(_changed.Count - 1);
                }
                else
                {
                    _last = null;
                }
            }

            public bool Contains(Object @object)
            {
                return _changed.Any(l => l.Contains(@object));
            }



        }


        class SaveChangeContext : Stacked<SaveChangeContext>
        {
            public int AttemptCount { get; set; }

        }
    }
}
