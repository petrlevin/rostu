using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.EntityFramework
{
    /// <summary>
    /// Базовый контекст
    /// </summary>
    public class DataContextBase:DbContext
    {
        public DataContextBase(ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        public DataContextBase(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public DataContextBase(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
        }

        public DataContextBase(string nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model)
        {
        }

        public DataContextBase(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        protected DataContextBase(DbCompiledModel model) : base(model)
        {
        }

        protected DataContextBase()
        {
        }

        protected virtual  void CustomOnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }
    }
}
