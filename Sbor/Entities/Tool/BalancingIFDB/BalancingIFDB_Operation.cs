using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaseApp;
using Platform.BusinessLogic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using SourcesData = Sbor.DbEnums.SourcesDataTools;
using EntityFramework.Extensions;


namespace Sbor.Tool
{
    /// <summary>
    /// Балансировка доходов, расходов и ИФДБ
    /// </summary>
    public partial class BalancingIFDB
    {
        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            //ExecuteControl(e => e.UniqueTest(context));

            DateLastEdit = DateTime.Now;

            var error = true;
            do
            {
                try
                {
                    var sc = context.BalancingIFDB
                        .Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation) // && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();
                    Number = sc.GetNextCode();

                    context.SaveChanges();
                    error = false;
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("Номер"))
                    {
                        throw;
                    }
                }
            } while (error);
        }

        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void BeforeEdit(DataContext context)
        {
            ExecuteControl(e => e.TestRole(context));
        }
        public void Edit(DataContext context)
        {
            //ExecuteControl(e => e.UniqueTest(context));

            DateLastEdit = DateTime.Now;
        }

        private string getFieldsEstLine(string a, string aKvr)
        {
            string res = string.Empty;

            foreach (var p in typeof (ILineCost).GetProperties())
            {
                var alias = (p.Name.Equals("IdKVR") ? aKvr : a);
                alias = (alias == null ? "" : alias + ".");
                res += string.Format("{0}{1}{2}", (string.IsNullOrEmpty(res) ? "" : ","), alias, p.Name);
            }

            return res;
        }

        private string getLinkNewEstLine(string l, string r, string rKvr)
        {
            string res = string.Empty;

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                res += string.Format(" and (isnull({0}.{2},0) = isnull({1}.{2},0)) ", l, (p.Name.Equals("IdKVR") ? rKvr : r), p.Name);
            }

            return res;
        }

        private string getFieldsToolsEstLine(DataContext context, string a)
        {
            var fields = context.EntityField.Where(w =>
                w.IdEntity == BalancingIFDB_EstimatedLine.EntityIdStatic 
                && !(new string[] {"IdEstimatedLine", "NewValue", "OldValue"}).Contains(w.Name)
            ).Select(s => s.Name).ToList();

            string res = string.Empty;

            foreach (var n in fields)
            {                
                res += string.Format("{0}{1}.{2}", (string.IsNullOrEmpty(res) ? "" : ","), a, n);
            }

            return res;
        }

        /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {
            ExecuteControl(e => e.UniqueTest(context));
            ExecuteControl(e => e.TestStatusOtherTools(context));

            bool isGroupKVR = IdBalancingIFDBType == (byte) Sbor.DbEnums.BalancingIFDBType.LimitBudgetAllocations
                              && context.BalancingIFDB_SetShowKBK.Any(a => a.IdOwner == Id && a.EntityField.Name.Equals("idKVR"));

            DateCommit = DateTime.Now;

            var q = context.BalancingIFDB_EstimatedLine.Where(w => 
                w.IdOwner == Id
                && ((w.NewValue ?? 0) - (w.OldValue ?? 0)) != 0
            );

            List<LimitVolumeAppropriations> lva = new List<LimitVolumeAppropriations>();

            foreach (var s in q)
            {
                lva.Add(new LimitVolumeAppropriations {
                    IdRegistratorEntity = EntityId,
                    IdRegistrator = Id,
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdBudget = IdBudget,
                    IdVersion = IdVersion,
                    IdTaskCollection = s.IdTaskCollection,
                    IdEstimatedLine = s.IdEstimatedLine,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = s.IdHierarchyPeriod,
                    IdAuthorityOfExpenseObligation =  s.IdAuthorityOfExpenseObligation,
                    IdOKATO = s.IdOKATO,
                    IdValueType = (IdSourcesDataTools == (byte)SourcesData.PublicInstitutionEstimate ? (byte)ValueType.BalancingIFDB_Estimate : (byte)ValueType.BalancingIFDB_ActivityOfSBP),
                    IsMeansAUBU = false,
                    HasAdditionalNeed = s.IsAdditionalNeed,
                    Value = (isGroupKVR ? -(s.OldValue ?? 0) : (s.NewValue ?? 0) - (s.OldValue ?? 0) ),
                    DateCreate = DateTime.Now 
                });
            }

            if (isGroupKVR)
            {
                string createNotExitsEstLine = string.Format(
                    "insert into reg.EstimatedLine (idActivityBudgetaryType, idPublicLegalFormation, idBudget, idSBP, " + getFieldsEstLine(null, null) + ") " +
                    "select distinct {3} as idActivityBudgetaryType, el.idPublicLegalFormation, el.idBudget, el.idSBP, " + getFieldsEstLine("el", "e") + " " +
                    "from tp.BalancingIFDB_EstimatedLine as t " +
                    "   inner join tp.BalancingIFDB_Expense as e on e.id = t.idMaster " +
                    "   inner join reg.EstimatedLine as el on el.id = t.idEstimatedLine " +
                    "   left join reg.EstimatedLine as nel on nel.idPublicLegalFormation = {1} and nel.idBudget = {2} and nel.idSBP = el.idSBP " + 
                        getLinkNewEstLine("nel", "el", "e") +
                    "where t.idOwner = {0} and nel.id is null and (isnull(t.NewValue,0)-isnull(t.OldValue,0)) != 0 ",
                    Id, IdPublicLegalFormation, IdBudget, (byte)ActivityBudgetaryType.Costs
                );
                context.Database.ExecuteSqlCommand(createNotExitsEstLine);

                string fields = getFieldsToolsEstLine(context, "t");
                string findEstLine = string.Format(
                    "select " + fields + ", nel.id as idEstimatedLine, sum(NewValue) as NewValue, sum(OldValue) as OldValue " +
                    "from tp.BalancingIFDB_EstimatedLine as t " +
                    "   inner join tp.BalancingIFDB_Expense as e on e.id = t.idMaster " +
                    "   inner join reg.EstimatedLine as el on el.id = t.idEstimatedLine " +
                    "   inner join reg.EstimatedLine as nel on nel.idPublicLegalFormation = {1} and nel.idBudget = {2} and nel.idSBP = el.idSBP " +
                        getLinkNewEstLine("nel", "el", "e") +
                    "where t.idOwner = {0} and (isnull(t.NewValue,0)-isnull(t.OldValue,0)) != 0 " +
                    "group by " + fields + ", nel.id ",
                    Id, IdPublicLegalFormation, IdBudget
                );
                var result2 = context.Database.SqlQuery<BalancingIFDB_EstimatedLine>(findEstLine).ToList();

                foreach (var s in result2)
                {
                    lva.Add(new LimitVolumeAppropriations
                    {
                        IdRegistratorEntity = EntityId,
                        IdRegistrator = Id,
                        IdPublicLegalFormation = IdPublicLegalFormation,
                        IdBudget = IdBudget,
                        IdVersion = IdVersion,
                        IdTaskCollection = s.IdTaskCollection,
                        IdEstimatedLine = s.IdEstimatedLine,
                        IsIndirectCosts = false,
                        IdHierarchyPeriod = s.IdHierarchyPeriod,
                        IdAuthorityOfExpenseObligation = s.IdAuthorityOfExpenseObligation,
                        IdOKATO = s.IdOKATO,
                        IdValueType = (IdSourcesDataTools == (byte)SourcesData.PublicInstitutionEstimate ? (byte)ValueType.BalancingIFDB_Estimate : (byte)ValueType.BalancingIFDB_ActivityOfSBP),
                        IsMeansAUBU = false,
                        HasAdditionalNeed = s.IsAdditionalNeed,
                        Value = s.NewValue ?? 0,
                        DateCreate = DateTime.Now
                    });
                }
            }

            context.LimitVolumeAppropriations.InsertAsTableValue(lva, context);
        }

        /// <summary>
        /// Операция «Отменить обработку»
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            context.LimitVolumeAppropriations.RemoveAll(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id);
            DateCommit = null;
        }

        /// <summary>
        /// Операция «Включить в свод»
        /// </summary>  
        public void IncludeInSetOf(DataContext context)
        {

        }

        /// <summary>
        /// Операция «Исключить из свода»
        /// </summary>  
        public void ExcludeFromTheSetOf(DataContext context)
        {

        }
    }
}
