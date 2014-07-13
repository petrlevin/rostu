using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.Log;
using Platform.SqlObjectModel.Extensions;
using IsolationLevel = System.Transactions.IsolationLevel;
using TransactionScope = Platform.BusinessLogic.DataAccess.TransactionScope;

namespace BaseApp.XmlExchange.Export
{
    /// <summary>
    /// 
    /// </summary>
    public class Exporter
    {
        private readonly TemplateExport _templateExport;
        private readonly TemplateImport _templateImport;

        private ConnectionState _initialConnectionState;
        private DataContext _dataContext;

        public Exporter(TemplateExport templateExport, TemplateImport templateImport, SqlConnection dbConnection = null, DbContext dbContext = null)
        {
            _templateExport = templateExport;
            _templateImport = templateImport;

            _dataContext = (dbContext ?? IoC.Resolve<DbContext>()).Cast<DataContext>();
            _dbConnection = dbConnection ?? _dataContext.Database.Connection;
        }

		public string Execute()
        {
            using (BeginTransaction())
            {
                try
                {

                    
                    using (var comm = _dbConnection.CreateCommand())
                    {
                        SelectStatement sourceSelect = BuildSourceSelect();
                        SelectStatement expRefsSelect = BuildExpRefsSelect();
                        CreateTableStatement refCreate = BuildRefCreate();

                        SelectStatement refSelect = BuildRefSelect();
                        SelectStatement sourceAndRefSelect = BuildSourceAndRefSelect(sourceSelect);
                        
                        InsertStatement refInsertFromSource = BuildRefInsert(sourceSelect, sourceSelect, expRefsSelect);
                        InsertStatement refInsert = BuildRefInsert(refSelect, sourceAndRefSelect, expRefsSelect);
                        IEnumerable<TSqlStatement> refFill = BuildRefFill(refInsert);

                        TSqlBatch batch = new TSqlBatch();
                        batch.Statements.Add(refCreate);
                        batch.Statements.Add(refInsertFromSource);
                        refFill.ToList().ForEach(s => batch.Statements.Add(s));

                        comm.CommandText = batch.Render();
                        comm.CommandTimeout = 0;
                        comm.ExecuteNonQueryLog();
                        comm.CommandText = BuildCreateResult(sourceAndRefSelect).Render();
                        comm.CommandTimeout = 0;

                        return comm.ExecuteScalarLog().ToString();
                    }
                }
                finally 
                {

                    if (_initialConnectionState == ConnectionState.Closed)
                    {
                        var objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;
                        objectContext.Connection.Close();
                    }
                }
            }

        }


        private TransactionScope BeginTransaction()
        {
            var objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;
            _initialConnectionState = objectContext.Connection.State;
            if (objectContext.Connection.State != ConnectionState.Closed)
                objectContext.Connection.Close();   
            SqlConnection.ClearAllPools();
            var result = new TransactionScope(TransactionScopeOption.RequiresNew,
                                              new TransactionOptions() { IsolationLevel = IsolationLevel.Snapshot });

            objectContext.Connection.Open();


            
            return result;



        }

        private IEnumerable<TSqlStatement> BuildRefFill(InsertStatement refInsert)
        {
            return new RefFillQueryBuilder(refInsert).Build();
        }

        private InsertStatement BuildRefInsert(SelectStatement sourceSelect, SelectStatement except, SelectStatement expRefSelect)
        {
            return new ReferencesQueryBuilder(sourceSelect).BuildInsert(refDestination, except, expRefSelect);
        }

        private SelectStatement BuildSourceAndRefSelect(SelectStatement sourceSelect)
        {
            return new ReferencesQueryBuilder(sourceSelect).BuildSourceAndRefs(refDestination);
        }

        private SelectStatement BuildRefSelect()
        {
            return ReferencesQueryBuilder.BuildRefsSelect(refDestination);
        }

        

        private static readonly SchemaObjectName refDestination = "#refferences".ToSchemaObjectName();
        private DbConnection _dbConnection;

        private CreateTableStatement BuildRefCreate()
        {
            return new CreateRefQueryBuilder().BuildCreate(refDestination);
        }

        private SelectStatement BuildSourceSelect()
        {
            return new SourceQueryBuilder(_templateExport).BuildSelect();
        }

        private SelectStatement BuildExpRefsSelect()
        {
            return new SourceQueryBuilder(_templateExport.AsSettingsForLinked()).BuildSelect();
        }


        private TSqlStatement BuildCreateResult(SelectStatement sourceWithRefSelect)
        {
            return new ResultQueryBuilder(sourceWithRefSelect, _templateImport).Build();
        }

    }
}
