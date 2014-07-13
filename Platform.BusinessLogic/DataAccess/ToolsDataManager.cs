using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Controls.DispatcherStrategies;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Exceptions;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Registry;
using Platform.ClientInteraction;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Базовый класс менеджера данных для работы с документами и интрументами
    /// </summary>
    public class ToolsDataManager: EFDataManager
    {

        #region Конструкторы

        public ToolsDataManager(SqlConnection dbConnection, IEntity source) : base(dbConnection, source)
        {
        }


        public ToolsDataManager(SqlConnection dbConnection, IEntity source, DbContext dbContext) : base(dbConnection, source, dbContext)
        {
        }
        
        #endregion

        #region Public

        /// <summary>
        /// Получить список общих атомарных операций
        /// </summary>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        public List<ResponseOperation> GetAvaliableAtomaricOperations(int[] itemIds)
        {
            var result = new List<ResponseOperation>();
            bool firstItem = true;

            foreach (var itemId in itemIds)
            {
                var doc = (IToolEntity)LoadBusinessEntity(itemId);
                var operations = doc.GetAvailableOperations().Select(s => new ResponseOperation
                {
                    id = s.Id,
                    name = s.Operation.Name,
                    text = s.Operation != null ? s.Operation.Caption : " ",
                    IsAtomic = !s.EditableFields.Any()
                }).Where(o=>o.IsAtomic).ToList();

                if (firstItem)
                {
                    firstItem = false;
                    result.AddRange(operations.ToList());
                }
                else
// ReSharper disable SimplifyLinqExpression
                    result.RemoveAll( operation => !operations.Any(o => o.name == operation.name) );
// ReSharper restore SimplifyLinqExpression
            }

            return result;
        }

        /// <summary>
        /// Начать неатомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="itemId">идентификатор документа или инструмента</param>
        /// <param name="entityOperationId">идентификатор операции</param>
        /// <exception cref="PlatformException"></exception>
        public void BeginOperation(int itemId, int entityOperationId)
        {
            try
            {
                using (var transaction = CreateTransaction())
                {
                    GetOperationDispatcher().BeginOperation(entityOperationId, LoadToolEntity(itemId));
                    DbContext.SaveChanges();
                    transaction.Complete();
                }
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка при начале выполнении операции . Идентификатор операции - {0}. Идентификатор экземпляра сущности {1}.  Обращайтесь к разработчику. ", entityOperationId, itemId), ex);
            }
        }

        /// <summary>
        /// Завершить начатую натомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="itemId">идентификатор документа или инструмента</param>
        /// <param name="values"></param>
        /// <exception cref="PlatformException"></exception>
        public ClientActionList CompleteOperation(int itemId , Dictionary<string,object> values)
        {
            try
            {
                using (var transaction = CreateTransaction())
                {
                    var toolEntity = LoadBusinessEntity(itemId);
                    using (new OperationWideContext(toolEntity))
                    {
                        if (values != null && values.Any())
                            toolEntity = DoUpdateEntity(values, toolEntity);
                        var result = GetOperationDispatcher().CompleteOperation((ToolEntity) toolEntity);
                        DbContext.SaveChanges();

                        transaction.Complete();
                        return result;
                    }
                }
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка при завершении операции .  Идентификатор экземпляра сущности {0}.  Обращайтесь к разработчику. ",  itemId), ex);
            }
        }

        /// <summary>
        /// Отменить начатую натомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="itemId">идентификатор документа или инструмента</param>
        /// <exception cref="PlatformException"></exception>
        public void CancelOperation(int itemId)
        {
            try
            {
                using (var transaction = CreateTransaction())
                {
                    GetOperationDispatcher().CancelOperation(LoadToolEntity(itemId));
                    DbContext.SaveChanges();
                    transaction.Complete();
                }
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка при отмене операции .  Идентификатор экземпляра сущности {0}.  Обращайтесь к разработчику. ", itemId), ex);
            }
        }



        /// <summary>
        /// Выполнить атомарную операцию для группы документов
        /// </summary>
        /// <param name="itemIds"></param>
        /// <param name="entityOperationId"></param>
        /// <exception cref="PlatformException"></exception>
        public void ExecuteGroupOperation(int[] itemIds, int entityOperationId)
        {
            try
            {
                var updateErrors = new List<String>();

                    var controlInteraction = IoC.Resolve<IControlInteraction>();

					foreach (var itemId in itemIds)
					{
						using (var transaction = CreateTransaction())
						{
							var businessEntity = LoadToolEntity(itemId);

							using (new OperationWideContext(businessEntity))
							{
								try
								{
									using (new ControlScope(new SkipSkippableStrategy(controlInteraction), false))
									{
										DoExecuteOperation(businessEntity, entityOperationId);
									}
									transaction.Complete();
								}
								catch (ControlResponseException ex)
								{
									var itemCaption =
										DbContext.Database.SqlQuery<string>(String.Format("Select dbo.GetCaption({0},{1})",
										                                                  Source.Id, itemId))
											.FirstOrDefault();
									updateErrors.Add(itemCaption + " - контроль №" + ex.UNK);
								}
								catch (ToolStateException ex)
								{
									updateErrors.Add(ex.Message);
								}
							}
						}
					}


	            if (updateErrors.Any())
                    throw new ControlResponseException("Выполнение операции над следующими элементами не завершено: <br/>" + string.Join(", <br/>", updateErrors), null, Source);
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка при выполнении операции . Идентификатор операции - {0}. Идентификаторы экземпляров сущности {1}.  Обращайтесь к разработчику. ", entityOperationId, string.Join(", ", itemIds) ), ex);
            }
        }

        /// <summary>
        /// Выполнить атомарную операцию для документа
        /// </summary>
        /// <param name="itemId">Идентификатор документа</param>
        /// <param name="entityOperationId">Идентификатор элемента справочника EntityOperation</param>
        /// <returns></returns>
        public ClientActionList ExecuteOperation(int itemId, int entityOperationId)
        {
            try
            {
                using (var transaction = CreateTransaction())
                {
                    var businessEntity = LoadToolEntity(itemId);
                    using (new OperationWideContext(businessEntity))
                    {
                        var result = DoExecuteOperation(businessEntity, entityOperationId);
                        transaction.Complete();
                        
                        return result;
                    }
                }
            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка при выполнении операции . Идентификатор операции - {0}. Идентификатор экземпляра сущности {1}.  Обращайтесь к разработчику. ", entityOperationId, itemId), ex);
            }
        }

        /// <summary>
        /// Заполнить респонс данными связанными с начатой операцией для документа или инструмента
        /// (текущая начатая операция и определенные для нее редактируемые поля документа или инструмента)
        /// </summary>
        /// <param name="appResponse"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public virtual ToolAppResponse FillStartedOperation(ToolAppResponse appResponse, int itemId)
        {
            throw new InvalidOperationException(String.Format("{0} не потдерживает начатые операции",GetType().FullName));
        }

        /// <summary>
        /// получить данные для эемента
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="fillStartedOperation">признак заполнения данными для начатой неатомарной операции 
        /// (идентификатор операции и определенные для нее редактируемые поля)
        ///  </param>
        /// <returns></returns>
        public  AppResponse GetEntityEntries(int itemId, bool fillStartedOperation)
        {
            var result = (ToolAppResponse)base.GetEntityEntries(itemId);
            var doc = (IToolEntity)LoadBusinessEntity(itemId);
            result.Operations = new ResponseOperations(doc.DocStatus == null ? "" : doc.DocStatus.Caption, doc.GetAvailableOperations());
            if (fillStartedOperation)
                FillStartedOperation(result, itemId);
            result.RegistryRecords = GetRegistryInfo(itemId);
            return result;
        }

        #endregion

        #region Public Override

        protected override void OnEntityCreated(IBaseEntity businessEntity)
        {
            try
            {
                GetOperationDispatcher().ProcessOperation("Create", (ToolEntity)businessEntity);
                DbContext.SaveChanges();

            }
            catch (OperationNotFoundException)
            {
            }
            
        }


        public override InitialItemState GetInitialState(Dictionary<string, object> clientDefaultsValues = null)
        {
            var result = base.GetInitialState();
            result.EditableFields = new List<string>() ;
            return result;
        }


        /// <summary>
        /// Получить элемент сущности по идентификатору
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public override AppResponse GetEntityEntries(int itemId)
        {
            var result = GetEntityEntries(itemId, true);
            return result;
        }
        
        #endregion

        #region Protected Override
        protected override InitialItemState CreateInitialState()
        {
            return base.CreateInitialState();
        }

        protected override AppResponse CreateAppResponce()
        {
            return new ToolAppResponse();
        }
        #endregion

        #region Protected
        protected ToolEntity LoadToolEntity(int itemId)
        {
            return (ToolEntity)LoadBusinessEntity(itemId);
        }

        #endregion

        #region Private

        

        private IOperationDispatcher _operationDispatcher;
        private IOperationDispatcher GetOperationDispatcher()
        {
            _operationDispatcher = _operationDispatcher ?? IoC.Resolve<IOperationDispatcher>();
            return _operationDispatcher;

        }




        private ClientActionList DoExecuteOperation(ToolEntity businessEntity, int entityOperationId)
        {
            var audit = Audit<OperationAuditor>.Start(new OperationAuditor()
                {
                    EntityId = Source.Id,
                    ElementId = businessEntity.Id,
                    OperationType = ProcessOperationTypes.EntityOperation,
                    OperationId = entityOperationId
                });
            
            var result = GetOperationDispatcher().ProcessOperation(entityOperationId, businessEntity);

            DbContext.SaveChanges();
            audit.Complete();

            return result;
        }

        private List<Registry.RecordsInfo> GetRegistryInfo(int docId)
        {
            return IoC.Resolve<RegistryManager>().GetInfo(Source, docId);
        }



        #endregion

    }
}
