using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Configuration;
using Platform.Common;
using Platform.Utils;

namespace Platform.BusinessLogic.DataAccess
{
    public  class TransactionScope :Stacked<TransactionScope> , IDisposable
    {
        private System.Transactions.TransactionScope _inner;
        // Summary:
        //     Initializes a new instance of the System.Transactions.TransactionScope class.
        public TransactionScope()
        {
            _inner = new System.Transactions.TransactionScope();
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Transactions.TransactionScope class
        //     and sets the specified transaction as the ambient transaction, so that transactional
        //     work done inside the scope uses this transaction.
        //
        // Parameters:
        //   transactionToUse:
        //     The transaction to be set as the ambient transaction, so that transactional
        //     work done inside the scope uses this transaction.
        public TransactionScope(Transaction transactionToUse)
        {
            _inner = new System.Transactions.TransactionScope(transactionToUse);
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Transactions.TransactionScope class
        //     with the specified requirements.
        //
        // Parameters:
        //   scopeOption:
        //     An instance of the System.Transactions.TransactionScopeOption enumeration
        //     that describes the transaction requirements associated with this transaction
        //     scope.
        public TransactionScope(TransactionScopeOption scopeOption)
        {
            _inner = new System.Transactions.TransactionScope(scopeOption);
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Transactions.TransactionScope class
        //     with the specified timeout value, and sets the specified transaction as the
        //     ambient transaction, so that transactional work done inside the scope uses
        //     this transaction.
        //
        // Parameters:
        //   transactionToUse:
        //     The transaction to be set as the ambient transaction, so that transactional
        //     work done inside the scope uses this transaction.
        //
        //   scopeTimeout:
        //     The System.TimeSpan after which the transaction scope times out and aborts
        //     the transaction.
        public TransactionScope(Transaction transactionToUse, TimeSpan scopeTimeout)
        {
            _inner = new System.Transactions.TransactionScope(transactionToUse, scopeTimeout);
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Transactions.TransactionScope class
        //     with the specified timeout value and requirements.
        //
        // Parameters:
        //   scopeOption:
        //     An instance of the System.Transactions.TransactionScopeOption enumeration
        //     that describes the transaction requirements associated with this transaction
        //     scope.
        //
        //   scopeTimeout:
        //     The System.TimeSpan after which the transaction scope times out and aborts
        //     the transaction.
        public TransactionScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            _inner = new System.Transactions.TransactionScope(scopeOption, scopeTimeout);
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Transactions.TransactionScope class
        //     with the specified requirements.
        //
        // Parameters:
        //   scopeOption:
        //     An instance of the System.Transactions.TransactionScopeOption enumeration
        //     that describes the transaction requirements associated with this transaction
        //     scope.
        //
        //   transactionOptions:
        //     A System.Transactions.TransactionOptions structure that describes the transaction
        //     options to use if a new transaction is created. If an existing transaction
        //     is used, the timeout value in this parameter applies to the transaction scope.
        //     If that time expires before the scope is disposed, the transaction is aborted.
        public TransactionScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            if (transactionOptions.Timeout == default(TimeSpan))
                transactionOptions.Timeout = TimeSpan.FromSeconds(int.Parse(WebConfigurationManager.AppSettings["TransactionTimeout"]));
            _inner = new System.Transactions.TransactionScope(scopeOption, transactionOptions);
        }
        // Summary:
        //     Indicates that all operations within the scope are completed successfully.
        //
        // Exceptions:
        //   System.InvalidOperationException:
        //     This method has already been called once.
        public void Complete()
        {
            _inner.Complete();
            OnAfterComplete();
        }
        
        /// <summary>
        /// Ends the transaction scope.
        /// </summary>
        public override void Dispose()
        {
            _inner.Dispose();
            OnAfterDisposed();
        }

        
        public event Action AfterComplete;
        /// <summary>
        /// 
        /// </summary>
        public event Action AfterDispose;

        protected virtual void OnAfterComplete()
        {
            var handler = AfterComplete;
            if (handler != null) handler();
        }

        protected virtual void OnAfterDisposed()
        {
            
            
            if ((Transaction.Current == null) ||
                (Transaction.Current.TransactionInformation.Status != TransactionStatus.Active))
            {
                var connection = IoC.Resolve<SqlConnection>("DbConnection");
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Open();
                }
            }
            var handler = AfterDispose;
            if (handler != null) handler();



        }





    }
}
