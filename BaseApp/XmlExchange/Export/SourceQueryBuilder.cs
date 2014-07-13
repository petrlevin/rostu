using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.DbEnums;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Extensions;

namespace BaseApp.XmlExchange.Export
{
    public class SourceQueryBuilder : BuilderBase
    {
        private IExportSettings _templateExport;



        public SourceQueryBuilder(IExportSettings templateExport, DbContext dbContext = null, SqlConnection dbConnection = null)
            : base(dbContext, dbConnection)
        {
            _templateExport = templateExport;

        }

        public SelectStatement BuildSelect()
        {
            var result = DoBuildSelect();
            return BuildLastVersion(result);
        }

        private SelectStatement BuildLastVersion(SelectStatement source)
        {
            if (source == null)
                return null;

            var result = new SelectStatement();
            source.CopyCtes(result);

            var query = Helper.CreateQuerySpecification(source.ToQueryDerivedTable("original"), "original.idEntity.idEntity",
                this.CreateSelectColumnForHierarhyDoc("original", "id", "idEntity", "id"), "original.id.originalId"
            );

            result.QueryExpression = query;
            return result;
        }

        private SelectStatement DoBuildSelect()
        {
            switch (_templateExport.SelectionType)
            {
                case SelectionType.EntitiesItemsSql:
                    {
                        return Helper.Parse<SelectStatement>(_templateExport.EntitiesSql);
                        break;
                    }
                case SelectionType.EntitiesSql:
                    {
                        return BuildForEntitiesSql();
                    }
                case SelectionType.Entities:
                    {
                        return BuildForEntities();
                    }
                case SelectionType.All:
                    {
                        if (_templateExport.TargetType == TargetType.Links)
                            return null;
                        throw new NotImplementedException("Выгрузка всех данных не реализована");
                    }
            }
            throw new NotImplementedException();
        }

        private SelectStatement BuildForEntities()
        {
            QueryExpression resultQuery = null;
            foreach (ISelectItem selectItem in _templateExport.Entities)
            {
                resultQuery = AddEntity(selectItem.Entity, resultQuery);
            }
            var result = new SelectStatement();

            result.QueryExpression = resultQuery;

            return result;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public SelectStatement BuildRefSelect()
        {
            switch (_templateExport.SelectionType)
            {
                case SelectionType.EntitiesItemsSql:
                        return Helper.Parse<SelectStatement>(_templateExport.EntitiesSql);
                case SelectionType.EntitiesSql:
                        return BuildForEntitiesSql();
            }
            
            throw new NotImplementedException();
        }



        private SelectStatement BuildForEntitiesSql()
        {
            var entities = _dataContext.Entity.ToList();
            using (var comm = _dbConnection.CreateCommand())
            {
                comm.CommandText = _templateExport.EntitiesSql;
                using (var reader = comm.ExecuteReaderLog())
                {

                    QueryExpression resultQuery = null;
                    while (reader.Read())
                    {
                        var entityId = reader.GetInt32(0);
                        var entity = entities.Single(e => e.Id == entityId);
                        resultQuery = AddEntity(entity, resultQuery);
                    }

                    var result = new SelectStatement();

                    result.QueryExpression = resultQuery;

                    return result;
                }


            }
        }

        private static QueryExpression AddEntity(Entity entity, QueryExpression resultQuery)
        {
            if (entity.EntityType == EntityType.Multilink)
                return resultQuery;

            var query = new QuerySpecification();
            query.FromClauses.Add(Helper.CreateSchemaObjectTableSource(entity.Schema, entity.Name,
                                                                       entity.Name));

            query.SelectElements.Add(Helper.CreateColumn(entity.Id.ToLiteral(),
                                                         "idEntity"));
            query.SelectElements.Add(Helper.CreateColumn(entity.Name, "id", "id"));
            resultQuery = resultQuery.UnionWith(query);
            return resultQuery;
        }
    }
}
