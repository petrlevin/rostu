using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Transactions;
using BaseApp;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Reference;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using SourcesData = Sbor.DbEnums.SourcesDataTools;
using TransactionScope = Platform.BusinessLogic.DataAccess.TransactionScope;


namespace Sbor.Tool
{
	/// <summary>
	/// Утверждение балансировки расходов, доходов и ИФДБ
	/// </summary>
	public partial class ApprovalBalancingIFDB
	{
        protected TransactionScope CreateTransaction()
        {
            var result = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                });

            return result;
        }

        /// <summary>   
        /// Провети документы ПОБА   
        /// </summary>  
        public void ProcessDocs_LimitBudgetAllocations(DataContext context, int[] ids = null)
        {
            DateCommit = DateTime.Now;

            var entity = Objects.ByName<Entity>(typeof(LimitBudgetAllocations).Name);

            using (new ControlScope(entity, true))
            {
                foreach (var d in LimitBudgetAllocations.Where(w => ids == null || ids.Contains(w.Id)))
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
                        if (d.IdDocStatus == DocStatus.Draft)
                        {
                            d.ExecuteOperation(e => e.Process(context));
                            context.SaveChanges();
                            d.ExecuteOperation(e => e.Confirm(context));
                            context.SaveChanges();
                        }
                        else if (d.IdDocStatus == DocStatus.Project)
                        {
                            d.ExecuteOperation(e => e.Confirm(context));
                            context.SaveChanges();
                        }

                        transaction.Complete();
                    }
                }
            }
        }

	    /// <summary>   
	    /// Отменить проведение документов ПОБА   
	    /// </summary>  
        public void UndoProcessDocs_LimitBudgetAllocations(DataContext context, int[] ids = null)
	    {
            var entity = Objects.ByName<Entity>(typeof(LimitBudgetAllocations).Name);

            using (new ControlScope(entity, true))
            {
                foreach (var d in LimitBudgetAllocations.Where(w => ids == null || ids.Contains(w.Id)))
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
                        if (d.IdDocStatus == DocStatus.Approved)
                        {
                            d.ExecuteOperation(e => e.UndoConfirm(context));
                            context.SaveChanges();
                            d.ExecuteOperation(e => e.UndoProcess(context));
                            context.SaveChanges();
                        }
                        else if (d.IdDocStatus == DocStatus.Project)
                        {
                            d.ExecuteOperation(e => e.UndoProcess(context));
                            context.SaveChanges();
                        }

                        transaction.Complete();
                    }
                }
            }
        }

        /// <summary>   
        /// Провети документы ДВ 
        /// </summary>  
        public void ProcessDocs_ActivityOfSBP(DataContext context, int[] ids = null)
        {
            DateCommit = DateTime.Now;

            var entity = Objects.ByName<Entity>(typeof(ActivityOfSBP).Name);

            using (new ControlScope(entity, true))
            {
                foreach (var d in ActivityOfSBPs.Where(w => ids == null || ids.Contains(w.Id)))
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
                        if (d.IdDocStatus == DocStatus.Draft)
                        {
                            d.ExecuteOperation(e => e.Process(context));
                            context.SaveChanges();
                            d.ExecuteOperation(e => e.Confirm(context));
                            context.SaveChanges();
                        }
                        else if (d.IdDocStatus == DocStatus.Project)
                        {
                            d.ExecuteOperation(e => e.Confirm(context));
                            context.SaveChanges();
                        }

                        transaction.Complete();
                    }
                }
            }
        }
    }
}