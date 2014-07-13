using System;
using System.Transactions;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using TransactionScope = Platform.BusinessLogic.DataAccess.TransactionScope;

namespace Platform.BusinessLogic.Auditing
{
    public class Audit<T> where T: IAuditor, new()
    {
        private AuditConfiguration Configuration { get; set; }

        public T Auditor { get; private set; }

        public bool IsRunning { get; private set; }

        public DateTime StartedAt { get; private set; }

        public DateTime CompletedAt { get; private set; }

        #region Public Statics

        // Do

        public static void Do(T auditor, Action<T> action, AuditTime when = AuditTime.Now)
        {
            var processor = new Audit<T>(auditor);
            if (!processor.preStart())
                return;

            processor.@do(() => action(auditor), when);
        }

        public static void Do(T auditor, Action<DateTime, T> action, AuditTime when = AuditTime.Now)
        {
            var processor = new Audit<T>(auditor);
            if (!processor.preStart())
                return;

            processor.@do(() => action(processor.StartedAt, auditor), when);
        }

        public static void Do(Action<T> action, AuditTime when = AuditTime.Now)
        {
            var processor = new Audit<T>(new T());
            if (!processor.preStart())
                return;

            processor.@do(() => action(processor.Auditor), when);
        }

        public static void Do(Action<DateTime, T> action, AuditTime when = AuditTime.Now)
        {
            var processor = new Audit<T>(new T());
            if (!processor.preStart())
                return;

            processor.@do(() => action(processor.StartedAt, processor.Auditor), when);
        }

        public static void Do(ISingleactionAuditor auditor, AuditTime when = AuditTime.Now)
        {
            var processor = new Audit<T>((T)auditor);
            if (!processor.preStart())
                return;

            processor.@do(() => auditor.Start(processor.StartedAt), when);
        }

        // Start. Необходимо принудительно вызвать завершающую часть аудита - метод Complete

        public static Audit<T> Start(Action<DateTime, T> action)
        {
            var processor = new Audit<T>(new T());
            if (!processor.preStart())
                return processor;

            action(processor.StartedAt, processor.Auditor);
            return processor;
        }

        public static Audit<T> Start(T auditor, Action<DateTime, T> action)
        {
            var processor = new Audit<T>(auditor);
            if (!processor.preStart())
                return processor;
            
            action(processor.StartedAt, processor.Auditor);
            return processor;
        }

        public static Audit<T> Start(IStartEndAuditor auditor)
        {
            var processor = new Audit<T>((T)auditor);
            if (!processor.preStart())
                return processor;

            auditor.Start(processor.StartedAt);
            return processor;
        }

        // Complete

        public void Complete(bool waitTransaction = false)
        {
            if (!(Auditor is ICompletableAuditor || Auditor is IStartEndAuditor))
                throw new PlatformException(string.Format("При вызове метода Complete аудитор должен реализовавать один из интерфейсов: {0}, {1}", typeof(ICompletableAuditor).Name, typeof(IStartEndAuditor).Name));

            completeAction(completeStarted, waitTransaction);
        }

        #endregion

        #region Private Methods

        private Audit(T auditor)
        {
            Configuration = IoC.Resolve<AuditConfiguration>();
            Auditor = auditor;
        }

        private bool preStart()
        {
            if (!Configuration.Enabled)
                return false;

            Auditor.Init();
            StartedAt = DateTime.Now;
            IsRunning = true;
            return true;
        }

        private void @do(Action action, AuditTime when)
        {
            switch (when)
            {
                case AuditTime.Now:
                    action();
                    break;
                case AuditTime.AfterTransaction:
                    completeAction(action, true);
                    break;
                case AuditTime.NowWithCompleteAfterTransaction:
                    action();
                    completeAction(completeStarted, true);
                    break;
            }
        }

        private void completeAction(Action action, bool waitTransaction)
        {
            if (!Configuration.Enabled)
                return;

            if (!waitTransaction)
                action();
            else
            {
                checkUseTransactionScope();
                var transaction = TransactionScope.Current;
                if (transaction == null)
                    action();
                else
                {
                    //Debug.WriteLine("Подписываемся на AfterComplete для {0}", transaction.GetHashCode());
                    transaction.AfterComplete += () => transaction.AfterDispose += action;
                }
                    
            }
        }

        private void completeStarted()
        {
            CompletedAt = DateTime.Now;
            int elapsedMilliseconds = Convert.ToInt32((CompletedAt - StartedAt).TotalMilliseconds);

            if (Auditor is IStartEndAuditor)
            {
                var auditor = (IStartEndAuditor) Auditor;
                auditor.End(elapsedMilliseconds);
            }
            else if (Auditor is ICompletableAuditor)
            {
                var auditor = (ICompletableAuditor)Auditor;
                auditor.Complete(elapsedMilliseconds);
            }

            IsRunning = false;
        }

        protected void checkUseTransactionScope()
        {
            if ((Transaction.Current != null) && (TransactionScope.Current == null))
                throw new InvalidOperationException(String.Format("Для выполненении аудита в транзакции транзакцию нужно инициализировать используя класс {0}.", typeof(TransactionScope).FullName));
        }

        #endregion
    }
}
