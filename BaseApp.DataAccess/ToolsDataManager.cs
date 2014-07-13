using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using BaseApp.Common.Interfaces;
using BaseApp.Reference;
using BaseApp.Registry;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Exceptions;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.DataAccess
{
    /// <summary>
    /// Дата менеджер для работы с инструментами и документами
    /// </summary>
    public class ToolsDataManager : Platform.BusinessLogic.DataAccess.ToolsDataManager
    {
        #region Приватные свойства

        private DataContext db
        {
            get { return DbContext.Cast<DataContext>(); }
        }

        private IUser _user;
        private IUser CurrentUser
        {
            get
            {
                if (_user == null)
                    _user = IoC.Resolve<IUser>("CurrentUser");
                return _user;                
            }
        }

        private User _efCurrentUser;
        /// <summary>
        /// Entity Framework Current User
        /// Объект текущего пользователя с возможностью обращения к ассоциациям
        /// </summary>
        private User efCurrentUser
        {
            get
            {
                if (_efCurrentUser == null)
                {
                    _efCurrentUser = db.User.Attach((User)CurrentUser);
                }
                return _efCurrentUser;
            }
        }

        private Role _operationAdmin;
        private Role operationAdmin
        {
            get
            {
                if (_operationAdmin == null)
                    _operationAdmin = Role.OperationAdmin;
                return _operationAdmin;
            }
        }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор для указанного типа сущности
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="source"></param>
        public ToolsDataManager(SqlConnection dbConnection, IEntity source)
            : base(dbConnection, source)
        {
        }

        /// <summary>
        /// Конструктор для указанного типа сущности и контекста
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="source"></param>
        /// <param name="dbContext"></param>
        public ToolsDataManager(SqlConnection dbConnection, IEntity source, DbContext dbContext)
            : base(dbConnection, source, dbContext)
        {
        }

        #endregion


        #region Private

        private  List<string> GetEditableFields(StartedOperation so)
        {
            if (so == null)
                return null;
            return so.EntityOperation.ActualEditableFields().Select(f => f.Name).ToList();
        }

        private StartedOperation GetStartedOperation(int itemId)
        {
            var so = db.StartedOperation.For(Source, itemId, StartedOperationInclude.EditableFields | StartedOperationInclude.Operation);
            return so;
        }

        #endregion


        #region Public override
        
        /// <summary>
        /// Получить элемент сущности по идентификатору
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public override AppResponse GetEntityEntries(int itemId)
        {
            var result = (ToolAppResponse)base.GetEntityEntries(itemId);
            var doc = (IToolEntity)LoadBusinessEntity(itemId);
            result.Operations = new ResponseOperations(doc.DocStatus == null ? "" : doc.DocStatus.Caption, doc.GetAvailableOperations());
            return result;
        }

        /// <summary>
        /// Заполнить респонс данными связанными с начатой операцией для документа или инструмента
        /// (текущая начатая операция и определенные для нее редактируемые поля документа или инструмента)
        /// </summary>
        /// <param name="appResponse"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public override ToolAppResponse FillStartedOperation(ToolAppResponse appResponse, int itemId)
        {
            var startedOperation = GetStartedOperation(itemId);
            if (startedOperation != null)
            {
                if (CurrentUser.Id != startedOperation.IdUser
                    && !CurrentUser.IsSuperUser()
                    && !efCurrentUser.Roles.Any(r => r == operationAdmin))
                    return appResponse; // считаем, что операция не началиналась. CORE-665. Если пользователь решит выполнить любую операцию - он получит сообщенеи об ошибке.
                
                appResponse.EditableFields = GetEditableFields(startedOperation);
                appResponse.CurrentOperationId = startedOperation.EntityOperation.Id;
                appResponse.ReadOnly = !CurrentUser.IsSuperUser() && CurrentUser.Id != startedOperation.IdUser;
            }
            return appResponse;
        }

        #endregion

        #region Protected override
        /// <summary>
        /// 
        /// </summary>
        /// <param name="businessEntity"></param>
        protected override void DoDeleteItem(IBaseEntity businessEntity)
        {
            CheckIsNotInOpperation(businessEntity);
            CheckIsDraftAndRoot(businessEntity);
            base.DoDeleteItem(businessEntity);
        }

        private void CheckIsDraftAndRoot(IBaseEntity businessEntity)
        {
            var document = (ToolEntity)businessEntity;
            if (document.IdDocStatus!= DocStatus.Draft)
                throw new ToolStateException(String.Format("Документ '{0}' имеет статус '{1}'. Удаление разрешено только для черновиков." ,document,document.DocStatus), document, "Удаление документа");
            var hDoc = document as IHierarhy;
            if (hDoc == null)
                return;
            if (hDoc.IdParent!=null)
                throw new ToolStateException(String.Format("У документа '{0}' есть предыдущия версия. Непосредственное удаление не возможно.", document), document, "Удаление документа");
        }

        private void CheckIsNotInOpperation(IBaseEntity businessEntity)
        {
            var startedOperations = db.StartedOperation;
            var document = (ToolEntity) businessEntity;
            if (startedOperations.Any(document))
            {
                var so = startedOperations.For(document, StartedOperationInclude.Operation);

                throw new ToolStateException(
                    String.Format("Над документом '{0}' выполняется операция '{1}'. Удаление не возможно.", document,
                                  so.EntityOperation.Operation.Caption), document, "Удаление документа");
            }
        }

        #endregion

    }

}
