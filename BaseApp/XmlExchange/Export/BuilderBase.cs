using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.XmlExchange.Export
{
    public class BuilderBase : IHasEntities

    {
        protected DataContext _dataContext;
        protected DbConnection _dbConnection;

        public BuilderBase(DbContext dbContext , DbConnection dbConnection)
        {
            _dataContext = (dbContext ?? IoC.Resolve<DbContext>()).Cast<DataContext>();
            _dbConnection = dbConnection ?? _dataContext.Database.Connection;

        }



        IQueryable<Entity> IHasEntities.Entities
        {
            get { return _dataContext.Entity; }
        }
    }
}
