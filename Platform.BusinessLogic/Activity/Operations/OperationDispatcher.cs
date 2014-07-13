using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Activity.Operations.Serialization;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Registry;
using Platform.ClientInteraction;
using Platform.Common;
using Platform.Common.Exceptions;

using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils.Extensions;
using ToolStatusException = Platform.BusinessLogic.Exceptions.ToolStatusException;

namespace Platform.BusinessLogic.Activity.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public class OperationDispatcher: OperationDispatcherBase, IOperationDispatcher
    {
        private readonly Locks _locks;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext">Контекст</param>
        /// <param name="locks">Блокировщик</param>
        /// <exception cref="ArgumentNullException"></exception>
        public OperationDispatcher([Dependency]DbContext dbContext, [Dependency]Locks locks) :base(dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            if (locks == null) throw new ArgumentNullException("locks");
            _locks = locks;
        }

        #region Private

        private DataContext GetDataContext()
        {
            return  DbContext.Cast<DataContext>();
        }


        private void CheckOriginalStatus(ToolEntity document, EntityOperation entityOperation)
        {
            if (document.DocStatus == null)
                throw new InvalidOperationException(String.Format("У документа {0} не  определен статус", document));
            if (!entityOperation.OriginalStatus.Any())
                return;
            if (entityOperation.OriginalStatus.Contains(document.DocStatus))
                return;
            throw new ToolStatusException(String.Format("Документ {0} имеет статус {1}. Выполнение операции {2} не возможно для этого статуса. ", document, document.DocStatus, entityOperation), document, document.DocStatus);
        }

        #endregion


        #region Override

        protected override void BlockDocument(ToolEntity document)
        {
            _locks.Lock(document);
        }


        protected override void CheckBeginState(ToolEntity document, EntityOperation entityOperation)
        {
            CheckOriginalStatus(document,entityOperation);
        }

        protected override void  CheckFinalStatus(ToolEntity document, EntityOperation entityOperation)
        {
            if (entityOperation.FinalStatus.Count() == 1)
            {
                document.DocStatus = entityOperation.FinalStatus.First();
            }
            else if (entityOperation.FinalStatus.Any())
            {
                if (entityOperation.FinalStatus.FirstOrDefault(ml => ml.Id == document.IdDocStatus) == null)
                    throw new ToolStatusException(
                        String.Format(
                            "Операция перевела документ '{0}' в недопустимый статус . Статус документа после выполнении операции  - {1} . Список допустимых статусов - {2}",
                            document, document.DocStatus, entityOperation.FinalStatus.ToString(",")), document, document.DocStatus
                        );
            }
            else if (document.DocStatus == null)
            {
                throw new ToolStatusException(
                    String.Format("Операция не установила статус  документу '{0}' ", document), document);
            }
        }

        protected override EntityOperation CheckCompleteState(ToolEntity document)
        {
            throw new InvalidOperationException(String.Format("Тип {0} не может быть использован для завершения не атомарной операции ",GetType()));
        }

        protected override EntityOperation CheckCancelState(ToolEntity document)
        {
            throw new InvalidOperationException(String.Format("Тип {0} не может быть использован для отмены не атомарной операции ", GetType()));
        }


		protected override void WriteEndOperation(ToolEntity document, EntityOperation entityOperation, bool withSerialized = true)
        {
            try
            {
                DbContext.Cast<DataContext>()
                         .SerializedEntityItem.Remove(
                             DbContext.Cast<DataContext>()
                                      .SerializedEntityItem.Single(
                                          sei => (sei.IdToolEntity == document.EntityId) && (sei.IdTool == document.Id)));

            }
            catch (Exception ex)
            {

                throw new PlatformException(String.Format("Ошибка удаления записи из регистра \"Сериализованный элемент сущности\". Документ - {0} ",document),ex);
            }
        }

        protected override void RestoreDocument(ToolEntity document)
        {
            try
            {
                IoC.Resolve<XmlDbSerializer>().RestoreFromRegistry(document);
            }
            catch (Exception ex)
            {

                throw new PlatformException(String.Format("Ошибка восстановления документа '{0}' из регистра \"Сериализованный элемент сущности\". ", document), ex);
            }
            
        }


        protected override EntityOperation GetOperation(int entityOperationId)
        {
            var dataContext = GetDataContext();
            var entityOperation = dataContext.EntityOperation.Include(e=>e.Operation).SingleOrDefault(e=>e.Id==entityOperationId);
            if (entityOperation == null)
                throw new OperationNotFoundException(String.Format("Операция  не найдена по идентификатору '{0}'.",
                                                          entityOperationId));
            return entityOperation;
        }

        protected override EntityOperation GetOperation(string operationName, int toolEntityId)
        {
            var dataContext = GetDataContext();
            try
            {
                var entityOperation =
                    dataContext.EntityOperation.Include(e => e.Operation)
                               .SingleOrDefault(e => (e.Operation.Name == operationName) && (e.IdEntity == toolEntityId));
                if (entityOperation == null)
                    throw new OperationNotFoundException(String.Format("Операция  не найдена по имени '{0}'.",
                                                              operationName));
                return entityOperation;

            }
            catch (InvalidOperationException ex)
            {
                throw new PlatformException(String.Format("Невозможно однозначно определить операцию по имени '{0}' для сущности {1}.",operationName,dataContext.Entity.Find(toolEntityId).Caption),ex);
            }
        }






		protected override void WriteBeginOperation(ToolEntity document, EntityOperation entityOperation, bool withSerialized = true)
        {
                IoC.Resolve<XmlDbSerializer>().SerializeToRegistry(document);    
        }

        #endregion
    }
}
