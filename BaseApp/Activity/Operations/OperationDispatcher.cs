using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using BaseApp.Reference;
using BaseApp.Registry;
using EntityFramework.Extensions;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Activity.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public class OperationDispatcher : Platform.BusinessLogic.Activity.Operations.OperationDispatcher
    {
        #region Private Properties

        private readonly IUser _curentUser;

        private DataContext db
        {
            get { return (DataContext)DbContext; }
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
                    _efCurrentUser = db.User.Attach((User)_curentUser);
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

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="locks">Блокировщик</param>
        /// <param name="curentUser"></param>
        public OperationDispatcher([Dependency]DbContext dbContext, [Dependency]Locks locks, [Dependency("CurrentUser")]IUser curentUser)
            : base(dbContext.Cast<DataContext>(), locks)
        {
            if (curentUser == null) throw new ArgumentNullException("curentUser");
            _curentUser = curentUser;
        }

        #endregion

        #region Override

        /// <summary>
        /// Выполнить операцию
        /// </summary>
        /// <param name="document"></param>
        /// <param name="entityOperation"></param>
        /// <returns></returns>
        protected override ClientActionList DoProcess(ToolEntity document, EntityOperation entityOperation)
        {

            var originalStatus = document.DocStatus;
            var executedOperation = WriteToExecutedOperation(document, entityOperation, originalStatus);
            using (new OperationContext(executedOperation))
            {
                var result = base.DoProcess(document, entityOperation);
                WriteToExecutedOperationEnd(executedOperation,document);
                return result;
            }
        }

        /// <summary>
        /// Проверка допустимости выполнения операции. 
        /// Возможность перехода по маршрутному дереву, отсутствие начатых неатомарных операций.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="entityOperation"></param>
        protected override void CheckBeginState(ToolEntity document, EntityOperation entityOperation)
        {
            base.CheckBeginState(document, entityOperation);
            CheckNotBegined(document);
        }

        /// <summary>
        /// Запись в регистры информации о начале операции
        /// </summary>
        /// <param name="document"></param>
        /// <param name="entityOperation"></param>
        /// <param name="withSerialized"></param>
		protected override void WriteBeginOperation(ToolEntity document, EntityOperation entityOperation, bool withSerialized = true)
        {
            WriteStartedOperationBegin(document, entityOperation);
            if (withSerialized)
				base.WriteBeginOperation(document, entityOperation);
        }

        /// <summary>
        /// Проверка допустимости выполнения действия "Применить"
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        protected override EntityOperation CheckCompleteState(ToolEntity document)
        {
            return CheckCancelCompleteState(document , "завершить");
        }

        /// <summary>
        /// Проверка допустимости выполнения действия "Отменить"
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        protected override EntityOperation CheckCancelState(ToolEntity document)
        {
            return CheckCancelCompleteState(document, "отменить");
        }

        /// <summary>
        /// Запись в регистры информации о завершении операции
        /// </summary>
        /// <param name="document"></param>
        /// <param name="entityOperation"></param>
        /// <param name="withSerialized"></param>
		protected override void WriteEndOperation(ToolEntity document, EntityOperation entityOperation, bool withSerialized = true)
        {
			if (withSerialized)
				base.WriteEndOperation(document, entityOperation);
            db.StartedOperation.Remove(db.StartedOperation.For(document));
        }

        #endregion

        #region Private

        private EntityOperation CheckCancelCompleteState(ToolEntity document , string cancelComplete)
        {
            var startedOperation = db.StartedOperation.For(document, StartedOperationInclude.EntityOperation);
            if (startedOperation == null)
                throw new ToolStateException(
                    String.Format("{0} не находится в состоянии начатой неатомарной операции.", document), document);
            var result = startedOperation.EntityOperation;
            if (_curentUser.IsSuperUser())
                return result;

            if (startedOperation.IdUser != _curentUser.Id && !efCurrentUser.Roles.Any(r => r == operationAdmin))
                throw new ToolStateException(
                    String.Format(
                        "Вы не можете {2} операцию '{0}', т.к. она была начата другим пользователем, " +
                        "а текущий пользователь не обладает правом администратора операций. " +
                        "Пользователь, начавший операцию: {1}",
                        startedOperation.EntityOperation, startedOperation.User,cancelComplete), document);
            return result;
        }



        private void WriteStartedOperationBegin(ToolEntity document, EntityOperation entityOperation)
        {
			try
			{
				using (var command = db.Database.Connection.CreateCommand())
				{
					if (db.Database.Connection.State!=ConnectionState.Open)
						db.Database.Connection.Open();
					command.CommandText =
						string.Format(
							@"SET LOCK_TIMEOUT 10;
						INSERT INTO [reg].[StartedOperation] ([idEntityOperation], [idRegistratorEntity], [idRegistrator], [idUser]) values ({0}, {1}, {2}, {3})
						SET LOCK_TIMEOUT -1;",
							entityOperation.Id, document.EntityId, document.Id, _curentUser.Id);
					command.ExecuteNonQuery();
				}
			}
			catch (Exception)
			{
				throw new LockEntityException(document, String.Format("Над объектом '{0}'  уже выполняется операция другим пользователем", document));
			}
	        /*
			var op = new StartedOperation()
            {
                IdRegistrator = document.Id,
                IdRegistratorEntity = document.EntityId,
                EntityOperation = entityOperation,
                IdUser = _curentUser.Id,
                Date = DateTime.Now
            };
            db.StartedOperation.Add(op);
			*/
        }

        private void CheckNotBegined(ToolEntity document)
        {
            if (db.StartedOperation.Any(document))
            {
                var so = db.StartedOperation.For(document, StartedOperationInclude.User);
                throw new ToolStateException(
                    String.Format(
                    "{0} редактируется пользователем: {1}", 
                    document, so.User), document);
            }
        }

        private ExecutedOperation WriteToExecutedOperation(ToolEntity document, EntityOperation entityOperation, DocStatus originalStatus)
        {
            var dataContext = (DataContext)DbContext;
            var result = dataContext.ExecutedOperation.Create();
            result.Date = DateTime.Now;
            result.OriginalStatus = originalStatus;
//            result.FinalStatus = document.DocStatus;
            result.IdUser = _curentUser.Id;
            result.EntityOperation = entityOperation;
            result.IdRegistratorEntity = document.EntityId;
            result.IdRegistrator = document.Id;
            dataContext.ExecutedOperation.Add(result);
            return result;
        }

        private void WriteToExecutedOperationEnd(ExecutedOperation executedOperation, ToolEntity document)
        {
            executedOperation.FinalStatus = document.DocStatus;
            executedOperation.Date = DateTime.Now;
        }

        #endregion
    }
}
