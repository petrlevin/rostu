using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.EntityTypes;
using Platform.Dal;
using Platform.Dal.Exceptions;
using Platform.Dal.Serialization;
using Platform.Log;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Activity.Operations.Serialization
{
    /// <summary>
    /// Сериализатор документа или инструмента в регистр (Сериализованный элемент сущности)
    /// </summary>
    public class XmlDbSerializer
    {

        private readonly SqlConnection _dbConnection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConnection"></param>
        public XmlDbSerializer([Dependency("DbConnection")] SqlConnection dbConnection)
        {
            if (dbConnection == null) throw new ArgumentNullException("dbConnection");
            _dbConnection = dbConnection;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        public void SerializeToRegistry(ToolEntity document)
        {
          SerializeToRegistry(Objects.ById<Entity>(document.EntityId),document.Id);
        }


        public void RestoreFromRegistry(ToolEntity document)
        {
            DoRestoreFromRegistry(Objects.ById<Entity>(document.EntityId), document.Id);
        }



        /// <summary>
        /// Сериализует сущность ы xml документ
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="XmlSerializationException"></exception>
        public void SerializeToRegistry(IEntity entity, int id)
        {
            try
            {
                DoSerialize(entity, id);
            }
            catch (XmlSerializationException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new XmlSerializationException(
                                            String.Format(
                                                        "Ошибка при сериализации сущности {0}. id - {1}. ",
                                                        entity.Caption, id),ex);

            }

        }


        private void DoRestoreFromRegistry(Entity entity, int id)
        {
            IncludeInTransaction();
            GetRestore(entity).Execute(id, _dbConnection);
        }


        private void DoSerialize(IEntity entity, int id)
        {
            var command = GetInsertSqlCommand(entity,id);
            command.Connection = _dbConnection;
            IncludeInTransaction();

            command.ExecuteNonQueryLog();
        }

        private void IncludeInTransaction()
        {
            if ((Transaction.Current != null) && (_dbConnection.State == ConnectionState.Open))
            {
                _dbConnection.Close();
                _dbConnection.Open();
            }
        }

        private SqlCommand GetInsertSqlCommand(IEntity entity, int id)
        {
            //var result = Cache.Get<String>(entity.Id);
            //if (result != null)
            //    return result;
            var result = BuildInsert(entity);
            //Cache.Put(result,entity.Id);
            //return result;
            return result.CreateCommand(id);
        }

        private RestoreCommands GetRestore(IEntity entity)
        {
            //var result = Cache.Get<String>(entity.Id);
            //if (result != null)
            //    return result;
            var result = BuildRestore(entity);
            //Cache.Put(result,entity.Id);
            //return result;
            return result;
        }




        private SerializationCommandFactory BuildInsert(IEntity entity)
        {
            return new InsertBuilder(entity).Build();
        }

        private RestoreCommands BuildRestore(IEntity entity)
        {
            return new RestoreBuilder(entity).Build();
        }



        //private static ISimpleCache Cache = new SimpleCache();
    }
}