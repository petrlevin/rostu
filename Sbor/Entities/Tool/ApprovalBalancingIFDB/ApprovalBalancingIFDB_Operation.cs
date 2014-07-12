using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using BaseApp;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
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
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using SourcesData = Sbor.DbEnums.SourcesDataTools;


namespace Sbor.Tool
{
	/// <summary>
	/// Утверждение балансировки расходов, доходов и ИФДБ
	/// </summary>
	public partial class ApprovalBalancingIFDB
	{
        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            DateLastEdit = DateTime.Now;


            var error = true;
            do
            {
                try
                {
                    var sc = context.ApprovalBalancingIFDB
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
        public void Edit(DataContext context)
        {
            if (IdDocStatus != DocStatus.CreateDocs)
            {
                DateLastEdit = DateTime.Now;
            }
        }

        private void UpdateBlanks(DataContext context)
        {
            string dt = DateTime.Now.Date.ToString("yyyy-MM-dd");

            context.Database.ExecuteSqlCommand(
                "declare @idTool int " +
                "declare @idBudget int " +
                "declare @idPublicLegalFormation int " +

                "set @idTool = {0} " +
                "set @idBudget = {1} " +
                "set @idPublicLegalFormation = {2} " +

                "declare @blanks table ( idBlank int, idToolBlank int ) " +

                "insert into @blanks " +
                "select b.id as idBlank, ib.id as idToolBlank " +
                "from ref.SBP as sbp " +
                "   inner join tp.SBP_Blank as b on b.idOwner = sbp.id " +
                "   inner join tp.ApprovalBalancingIFDB_Blank as ib on ib.idOwner = @idTool and ib.idBlankType = b.idBlankType " +
                "where sbp.idPublicLegalFormation = @idPublicLegalFormation  " +
                "   and (sbp.ValidityTo is null or sbp.ValidityTo >= '"+dt+"') " +
                "   and (sbp.ValidityFrom <= '"+dt+"') " +
                "   and b.idBudget = @idBudget " +
                "   and not (isnull(ib.idBlankValueType_ExpenseObligationType, 0) = isnull(b.idBlankValueType_ExpenseObligationType, 0) " +
                "   and isnull(ib.idBlankValueType_FinanceSource, 0) = isnull(b.idBlankValueType_FinanceSource, 0) " +
                "   and isnull(ib.idBlankValueType_KFO, 0) = isnull(b.idBlankValueType_KFO, 0) " +
                "   and isnull(ib.idBlankValueType_KVSR, 0) = isnull(b.idBlankValueType_KVSR, 0) " +
                "   and isnull(ib.idBlankValueType_RZPR, 0) = isnull(b.idBlankValueType_RZPR, 0) " +
                "   and isnull(ib.idBlankValueType_KCSR, 0) = isnull(b.idBlankValueType_KCSR, 0) " +
                "   and isnull(ib.idBlankValueType_KVR, 0) = isnull(b.idBlankValueType_KVR, 0) " +
                "   and isnull(ib.idBlankValueType_KOSGU, 0) = isnull(b.idBlankValueType_KOSGU, 0) " +
                "   and isnull(ib.idBlankValueType_DFK, 0) = isnull(b.idBlankValueType_DFK, 0) " +
                "   and isnull(ib.idBlankValueType_DKR, 0) = isnull(b.idBlankValueType_DKR, 0) " +
                "   and isnull(ib.idBlankValueType_DEK, 0) = isnull(b.idBlankValueType_DEK, 0) " +
                "   and isnull(ib.idBlankValueType_CodeSubsidy, 0) = isnull(b.idBlankValueType_CodeSubsidy, 0) " +
                "   and isnull(ib.idBlankValueType_BranchCode, 0) = isnull(b.idBlankValueType_BranchCode, 0)) " +

                "update b set " +
                "   b.idBlankValueType_ExpenseObligationType = ib.idBlankValueType_ExpenseObligationType, " +
                "   b.idBlankValueType_FinanceSource = ib.idBlankValueType_FinanceSource, " +
                "   b.idBlankValueType_KFO = ib.idBlankValueType_KFO, " +
                "   b.idBlankValueType_KVSR = ib.idBlankValueType_KVSR, " +
                "   b.idBlankValueType_RZPR = ib.idBlankValueType_RZPR, " +
                "   b.idBlankValueType_KCSR = ib.idBlankValueType_KCSR, " +
                "   b.idBlankValueType_KVR = ib.idBlankValueType_KVR, " +
                "   b.idBlankValueType_KOSGU = ib.idBlankValueType_KOSGU, " +
                "   b.idBlankValueType_DFK = ib.idBlankValueType_DFK, " +
                "   b.idBlankValueType_DKR = ib.idBlankValueType_DKR, " +
                "   b.idBlankValueType_DEK = ib.idBlankValueType_DEK, " +
                "   b.idBlankValueType_CodeSubsidy = ib.idBlankValueType_CodeSubsidy, " +
                "   b.idBlankValueType_BranchCode = ib.idBlankValueType_BranchCode " +
                "from @blanks as t " +
                "   inner join tp.SBP_Blank as b on b.id = t.idBlank " +
                "   inner join tp.ApprovalBalancingIFDB_Blank as ib on ib.id = t.idToolBlank " +

                "insert into tp.SBP_BlankHistory ( " +
                "   idOwner, " +
                "   DateCreate, " +
                "   idBudget, " +
                "   idBlankType, " +
                "   idBlankValueType_ExpenseObligationType, " +
                "   idBlankValueType_FinanceSource, " +
                "   idBlankValueType_KFO, " +
                "   idBlankValueType_KVSR, " +
                "   idBlankValueType_RZPR, " +
                "   idBlankValueType_KCSR, " +
                "   idBlankValueType_KVR, " +
                "   idBlankValueType_KOSGU, " +
                "   idBlankValueType_DFK, " +
                "   idBlankValueType_DKR, " +
                "   idBlankValueType_DEK, " +
                "   idBlankValueType_CodeSubsidy, " +
                "   idBlankValueType_BranchCode) " +
                "select b.idOwner, " +
                "   GETDATE(), " +
                "   @idBudget, " +
                "   b.idBlankType, " +
                "   b.idBlankValueType_ExpenseObligationType, " +
                "   b.idBlankValueType_FinanceSource, " +
                "   b.idBlankValueType_KFO, " +
                "   b.idBlankValueType_KVSR, " +
                "   b.idBlankValueType_RZPR, " +
                "   b.idBlankValueType_KCSR, " +
                "   b.idBlankValueType_KVR, " +
                "   b.idBlankValueType_KOSGU, " +
                "   b.idBlankValueType_DFK, " +
                "   b.idBlankValueType_DKR, " +
                "   b.idBlankValueType_DEK, " +
                "   b.idBlankValueType_CodeSubsidy, " +
                "   b.idBlankValueType_BranchCode " +
                "from @blanks as t " +
                "   inner join tp.SBP_Blank as b on b.id = t.idBlank",
            Id, IdBudget, IdPublicLegalFormation);
        }

        private void CreateLBA(DataContext context)
        {
            string query = string.Format(
                "declare @idTool int " +
                "declare @idBudget int " +
                "declare @idPublicLegalFormation int " +
                "declare @idVersion int " +
                "declare @idEntityBalancingIFDB int " +
                "declare @budgetYear int " +

                "set @idTool = {0} " +
                "set @idBudget = {1} " +
                "set @idPublicLegalFormation = {2} " +
                "set @idVersion = {3} " +
                "set @idEntityBalancingIFDB = {4} " +
                "set @budgetYear = {5} " +

                "declare @SBPs table ( id int ) " +

                "insert into @SBPs " +
                "select distinct d.idSBP " +
                "from doc.LimitBudgetAllocations as d " +
                "   inner join tp.SBP_BlankHistory as b on b.id = d.idSBP_BlankActual " +
                "   left join tp.ApprovalBalancingIFDB_Blank as ib on (ib.idBlankType = {6} " +
                "       and isnull(ib.idBlankValueType_ExpenseObligationType, 0) = isnull(b.idBlankValueType_ExpenseObligationType, 0) " +
                "       and isnull(ib.idBlankValueType_FinanceSource, 0) = isnull(b.idBlankValueType_FinanceSource, 0) " +
                "       and isnull(ib.idBlankValueType_KFO, 0) = isnull(b.idBlankValueType_KFO, 0) " +
                "       and isnull(ib.idBlankValueType_KVSR, 0) = isnull(b.idBlankValueType_KVSR, 0) " +
                "       and isnull(ib.idBlankValueType_RZPR, 0) = isnull(b.idBlankValueType_RZPR, 0) " +
                "       and isnull(ib.idBlankValueType_KCSR, 0) = isnull(b.idBlankValueType_KCSR, 0) " +
                "       and isnull(ib.idBlankValueType_KVR, 0) = isnull(b.idBlankValueType_KVR, 0) " +
                "       and isnull(ib.idBlankValueType_KOSGU, 0) = isnull(b.idBlankValueType_KOSGU, 0) " +
                "       and isnull(ib.idBlankValueType_DFK, 0) = isnull(b.idBlankValueType_DFK, 0) " +
                "       and isnull(ib.idBlankValueType_DKR, 0) = isnull(b.idBlankValueType_DKR, 0) " +
                "       and isnull(ib.idBlankValueType_DEK, 0) = isnull(b.idBlankValueType_DEK, 0) " +
                "       and isnull(ib.idBlankValueType_CodeSubsidy, 0) = isnull(b.idBlankValueType_CodeSubsidy, 0) " +
                "       and isnull(ib.idBlankValueType_BranchCode, 0) = isnull(b.idBlankValueType_BranchCode, 0)) " +
                "where " +
                "   d.idBudget = @idBudget " +
                "   and d.idVersion = @idVersion " +
                "   and d.idPublicLegalFormation = @idPublicLegalFormation " +
                "   and d.idDocStatus in ({7},{8},{9}) " +
                "   and ib.id is null " +
                "union " +
                "select distinct " +
                "   sbp.id " +
                "from " +
                "   (select distinct el.idSBP from" +
                "   ml.ApprovalBalancingIFDB_BalancingIFDB as m " +
                "   inner join reg.LimitVolumeAppropriations as r on r.idRegistrator = m.idBalancingIFDB and r.idRegistratorEntity = @idEntityBalancingIFDB " +
                "   inner join reg.EstimatedLine as el on el.id = r.idEstimatedLine " +
                "   where m.idApprovalBalancingIFDB = @idTool) as x " +
                "   cross apply dbo.GetParents(x.idSBP,'-2013265747','SBP','idParent',1) as s " +
			    "   inner join ref.SBP as sbp on sbp.id = s.id " +
                "where sbp.idSBPType in ({11}, {12}) " +

                "select d.* " +
                "from @SBPs as t inner join doc.LimitBudgetAllocations as d on d.idSBP = t.id " +
                "where " +
                "   d.idBudget = @idBudget " +
                "   and d.idVersion = @idVersion " +
                "   and d.idPublicLegalFormation = @idPublicLegalFormation " +
                "   and d.idDocStatus in ({7},{8},{10}) ",

                Id, IdBudget, IdPublicLegalFormation, IdVersion, BalancingIFDB.EntityIdStatic, Budget.Year,
                (byte)BlankType.BringingGRBS,
                DocStatus.Project, DocStatus.Approved, DocStatus.Changed, DocStatus.Draft,
                (byte)SBPType.GeneralManager, (byte)SBPType.Manager
            );

            string sql = string.Empty;

            var docs = context.LimitBudgetAllocations.SqlQuery(query).ToList();
            foreach (var t in docs)
            {
                int idDoc = t.Id;
                if (t.IdDocStatus != DocStatus.Draft)
                {
                    t.ExecuteOperation(e => e.Change(context));
                    idDoc = context.LimitBudgetAllocations.Where(s => s.IdParent == t.Id).Select(s => s.Id).Single();
                }
                else
                {
                    t.SetBlankActual(context);
                }
                sql += string.Format(
                    " insert into ml.ApprovalBalancingIFDB_LimitBudgetAllocations (idApprovalBalancingIFDB,idLimitBudgetAllocations) values ({0},{1}) ",
                    Id, idDoc
                );
            }
            context.SaveChanges();

            if (!string.IsNullOrEmpty(sql))
            {
                context.Database.ExecuteSqlCommand(sql);
                context.Database.ExecuteSqlCommand(
                    "delete tp.LimitBudgetAllocations_LimitAllocations where idOwner in (" +
                    "select idLimitBudgetAllocations from ml.ApprovalBalancingIFDB_LimitBudgetAllocations where idApprovalBalancingIFDB = {0})",
                    Id);
            }
        }

	    private void FillLBA(DataContext context)
	    {

            string fields = string.Empty;
            string fields2 = string.Empty;

            ApprovalBalancingIFDB_Blank bl = Blanks.Single(w => w.IdBlankType == (byte)BlankType.BringingGRBS);
            foreach (var p in typeof(ISBP_Blank).GetProperties())
	        {
                if (((byte?) bl.GetValue(p.Name) ?? (byte) BlankValueType.Empty) != (byte) BlankValueType.Empty)
                {
                    fields += p.Name.Replace("IdBlankValueType_", "Id") + ",";
                    fields2 += p.Name.Replace("IdBlankValueType_", "el.Id") + ",";
                }
	        }

            string query = string.Format(
                "declare @idTool int " +
                "declare @idBudget int " +
                "declare @idPublicLegalFormation int " +
                "declare @idVersion int " +
                "declare @budgetYear int " +

                "set @idTool = {0} " +
                "set @idBudget = {1} " +
                "set @idPublicLegalFormation = {2} " +
                "set @idVersion = {3} " +
                "set @budgetYear = {4} " +

	            "declare @SBPs table (idSBP int, idChidrenSBP int) " +
                "declare @ValueTypes table (id tinyint) " +

		        "insert into @SBPs " +
		        "select d.idSBP as idSBP, sbp.id as idChidrenSBP " +
		        "from ml.ApprovalBalancingIFDB_LimitBudgetAllocations as m " +
			    "    inner join doc.LimitBudgetAllocations as d on d.id = m.idLimitBudgetAllocations " +
                (
                    IdSourcesDataTools == (byte)SourcesDataTools.PublicInstitutionEstimate 
                    ? "cross apply dbo.GetChildrens(d.idSBP, -2013265747, 1) as s inner join ref.SBP as sbp on sbp.id = s.id and sbp.idSBPType = {5} "
                    : "inner join ref.SBP as sbp on (sbp.id = d.idSBP or (sbp.idParent = d.idSBP and sbp.idSBPType = {6})) "
                ) +
                "where m.idApprovalBalancingIFDB = @idTool " +
                (
                    IdSourcesDataTools == (byte)SourcesDataTools.PublicInstitutionEstimate 
                    ? "insert into @ValueTypes select {7} union all select {8} "
                    : "insert into @ValueTypes select {9} union all select {10} "
                ) +

                "insert into tp.LimitBudgetAllocations_LimitAllocations (idOwner," + fields + "OFG,PFG1,PFG2) " +
                "select d.id," + fields2 +
                "   sum(case when year(hp.DateStart) = @budgetYear+0 then lva.Value else null end) as OFG, " +
                "   sum(case when year(hp.DateStart) = @budgetYear+1 then lva.Value else null end) as PFG1, " +
                "   sum(case when year(hp.DateStart) = @budgetYear+2 then lva.Value else null end) as PFG2 " +
                "from reg.LimitVolumeAppropriations as lva " +
		        "   inner join ref.HierarchyPeriod as hp on hp.id = lva.idHierarchyPeriod " +
		        "   inner join reg.EstimatedLine as el on el.id = lva.idEstimatedLine " +
		        "   inner join @SBPs as f on f.idChidrenSBP = el.idSBP " +
		        "   inner join @ValueTypes as fv on fv.id = lva.idValueType " +
		        "   inner join doc.LimitBudgetAllocations as d on d.idSBP = f.idSBP " +
		        "   inner join ml.ApprovalBalancingIFDB_LimitBudgetAllocations as m on m.idApprovalBalancingIFDB = @idTool and m.idLimitBudgetAllocations = d.id " +
                "where " +
                "   lva.idPublicLegalFormation = @idPublicLegalFormation " +
		        "   and lva.idBudget = @idBudget " +
		        "   and lva.idVersion = @idVersion " +
                "   and isnull(lva.HasAdditionalNeed,0) = 0 " +
                "   and year(hp.DateStart) between @budgetYear and @budgetYear+2 " +
                "group by " + fields2 + "d.id " +
                "having " +
                "   sum(case when year(hp.DateStart) = @budgetYear+0 then lva.Value else 0 end) != 0 " +
                "   or sum(case when year(hp.DateStart) = @budgetYear+1 then lva.Value else 0 end) != 0 " +
                "   or sum(case when year(hp.DateStart) = @budgetYear+2 then lva.Value else 0 end) != 0 ",

                Id, IdBudget, IdPublicLegalFormation, IdVersion, Budget.Year,
                (byte)SBPType.TreasuryEstablishment, (byte)SBPType.Manager,
                (byte)ValueType.Justified, (byte)ValueType.BalancingIFDB_Estimate,
                (byte)ValueType.JustifiedGRBS, (byte)ValueType.BalancingIFDB_ActivityOfSBP
            );

            context.Database.ExecuteSqlCommand(query);
        }

        private ActivityOfSBP CloneActivityOfSbp(DataContext context, ActivityOfSBP doc)
        {
            Clone cloner = new Clone(doc);
            ActivityOfSBP newDoc = (ActivityOfSBP)cloner.GetResult();
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = doc.Id;
            newDoc.DateCommit = null;
            newDoc.DateTerminate = null;
            newDoc.HasAdditionalNeed = doc.HasAdditionalNeed;
            newDoc.DateCommit = null;
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.DocType = doc.DocType;

            string[] nn = doc.Number.Split('.');
            if (nn.Count() == 1) nn = (doc.Number+".0").Split('.');
            int cc = nn.Count();
            nn[cc - 1] = (cc > 1 ? int.Parse(nn[cc - 1]) + 1 : 1).ToString(CultureInfo.InvariantCulture);  

            newDoc.Number = string.Join(".", nn);

            newDoc.Header = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;

            doc.IdDocStatus = DocStatus.Changed;

            return newDoc;
        }

        private void CreateAoSBP(DataContext context)
        {
            string query = string.Format(
                "declare @idTool int " +
                "declare @idBudget int " +
                "declare @idPublicLegalFormation int " +
                "declare @idVersion int " +
                "declare @idEntityBalancingIFDB int " +
                "declare @idEntityActivityOfSBP int " +
                "declare @budgetYear int " +
                "declare @idSourcesDataTools int " +

                "set @idTool = {0} " +
                "set @idBudget = {1} " +
                "set @idPublicLegalFormation = {2} " +
                "set @idVersion = {3} " +
                "set @idEntityBalancingIFDB = {4} " +
                "set @idEntityActivityOfSBP = {5} " +
                "set @budgetYear = {6} " +
                "set @idSourcesDataTools = {10} " +

                "declare @doc1 table (id int) " +

                "insert into @doc1 " +
                "select distinct d.id " +
                "from doc.ActivityOfSBP as d " +
                "   inner join tp.ActivityOfSBP_SBPBlankActual as a on a.idOwner = d.id " +
                "   inner join tp.SBP_BlankHistory as b on b.id = a.idSBP_BlankHistory and b.idBudget = @idBudget and b.idBlankType = {7} " +
                "   left join tp.ApprovalBalancingIFDB_Blank as ib on (ib.idBlankType = {7} " +
                "       and isnull(ib.idBlankValueType_ExpenseObligationType, 0) = isnull(b.idBlankValueType_ExpenseObligationType, 0) " +
                "       and isnull(ib.idBlankValueType_FinanceSource, 0) = isnull(b.idBlankValueType_FinanceSource, 0) " +
                "       and isnull(ib.idBlankValueType_KFO, 0) = isnull(b.idBlankValueType_KFO, 0) " +
                "       and isnull(ib.idBlankValueType_KVSR, 0) = isnull(b.idBlankValueType_KVSR, 0) " +
                "       and isnull(ib.idBlankValueType_RZPR, 0) = isnull(b.idBlankValueType_RZPR, 0) " +
                "       and isnull(ib.idBlankValueType_KCSR, 0) = isnull(b.idBlankValueType_KCSR, 0) " +
                "       and isnull(ib.idBlankValueType_KVR, 0) = isnull(b.idBlankValueType_KVR, 0) " +
                "       and isnull(ib.idBlankValueType_KOSGU, 0) = isnull(b.idBlankValueType_KOSGU, 0) " +
                "       and isnull(ib.idBlankValueType_DFK, 0) = isnull(b.idBlankValueType_DFK, 0) " +
                "       and isnull(ib.idBlankValueType_DKR, 0) = isnull(b.idBlankValueType_DKR, 0) " +
                "       and isnull(ib.idBlankValueType_DEK, 0) = isnull(b.idBlankValueType_DEK, 0) " +
                "       and isnull(ib.idBlankValueType_CodeSubsidy, 0) = isnull(b.idBlankValueType_CodeSubsidy, 0) " +
                "       and isnull(ib.idBlankValueType_BranchCode, 0) = isnull(b.idBlankValueType_BranchCode, 0)) " +
                "   left join doc.ActivityOfSBP as p on p.idParent = d.id " +
                "where " +
                "   p.id is null " +
                "   and d.idVersion = @idVersion " +
                "   and d.idPublicLegalFormation = @idPublicLegalFormation " +
                "   and ib.id is null " +

                "declare @doc2tmp table (id int) " +

                "insert into @doc2tmp " +
                "select distinct tv.idRegistrator " +
                "from (select distinct " +
				"   case @idSourcesDataTools when 1 then sbp.idParent else sbp.id end as idSBP, " +
				"   r.idTaskCollection, r.idHierarchyPeriod " +
                "   from ml.ApprovalBalancingIFDB_BalancingIFDB as m " +
                "   inner join tool.BalancingIFDB as t on t.id = m.idBalancingIFDB " +
                "   inner join reg.LimitVolumeAppropriations as r on r.idRegistrator = m.idBalancingIFDB and r.idRegistratorEntity = @idEntityBalancingIFDB " +
                "   inner join reg.EstimatedLine as el on el.id = r.idEstimatedLine " +
                "   inner join ref.SBP as sbp on sbp.id = el.idSBP " +
                "   where m.idApprovalBalancingIFDB = @idTool " +
                "   and t.idBalancingIFDBType = {8}) as x " +
                "left join reg.TaskVolume as tv on " +
                "   tv.idTaskCollection = x.idTaskCollection and tv.idHierarchyPeriod = x.idHierarchyPeriod and tv.idSBP = x.idSBP " +
                "   and tv.idTerminator is null and tv.idRegistratorEntity = @idEntityActivityOfSBP and tv.idValueType = {9} " +


                "declare @doc2 table (id int, idDocStatus int); " +

                "with cte as ( " +
                "   select id from @doc2tmp " +
                "   union all " +
                "   select d.id from cte inner join doc.ActivityOfSBP as d on d.idParent = cte.id) " +

                "insert into @doc2 " +
                "select distinct d.id, d.idDocStatus " +
                "from cte inner join doc.ActivityOfSBP as d on d.id = cte.id " +
                "   left join doc.ActivityOfSBP as p on p.idParent = d.id " +
                "where p.id is null " +

                "update d set " +
                "   d.isRequireClarification = 1, " +
                "   d.ReasonClarification = case when d.ReasonClarification is null then '' else '\r\n' end + " + 
                    "'" + DateTime.Now.ToString("dd.MM.yyyy") + " был изменен бланк «Формирование ГРБС» необходимо уточнить КБК в строках расходов по мероприятиям' " +
                "from " +
                "   @doc1 as d1 " +
                "   left join @doc2 as d2 on d2.id = d1.id " +
                "   inner join doc.ActivityOfSBP as d on d.id = d1.id " +
                "where d2.id is null " +

                "select d.* " +
                "from @doc2 as d2 inner join doc.ActivityOfSBP as d on d.id = d2.id ",

                Id, IdBudget, IdPublicLegalFormation, IdVersion, BalancingIFDB.EntityIdStatic, ActivityOfSBP.EntityIdStatic, Budget.Year,
                (byte)BlankType.FormationGRBS,
                (byte)BalancingIFDBType.LimitBudgetAllocationsAndActivityOfSBP, (byte)ValueType.Plan,
                IdSourcesDataTools
            );

            string sql = string.Empty;

            var docs = context.ActivityOfSBP.SqlQuery(query).ToList();
            foreach (var t in docs)
            {
                int idDoc = t.Id;
                if (t.IdDocStatus != DocStatus.Draft)
                {
                    var doc = CloneActivityOfSbp(context, t);
                    context.SaveChanges();
                    idDoc = doc.Id;

                    //t.ExecuteOperation(e => e.Change(context));
                    //idDoc = context.ActivityOfSBP.Where(s => s.IdParent == t.Id).Select(s => s.Id).Single();
                }
                else
                {
                    t.SetBlankActual(context);
                    context.SaveChanges();
                }
                sql += string.Format(
                    " insert into ml.ApprovalBalancingIFDB_ActivityOfSBP (idApprovalBalancingIFDB,idActivityOfSBP) values ({0},{1}) ",
                    Id, idDoc
                );
            }

            if (!string.IsNullOrEmpty(sql))
            {
                context.Database.ExecuteSqlCommand(sql);
                context.Database.ExecuteSqlCommand(
                    "delete tp.ActivityOfSBP_ActivityResourceMaintenance where idOwner in (" +
                    "select idActivityOfSBP from ml.ApprovalBalancingIFDB_ActivityOfSBP where idApprovalBalancingIFDB = {0}" +
                    ") and idBudget = {1}",
                    Id, IdBudget);
            }
        }

        private void FillAoSBP(DataContext context)
        {

            string fields = string.Empty;
            string fields2 = string.Empty;
            string kbkEq = string.Empty;

            ApprovalBalancingIFDB_Blank bl = Blanks.Single(w => w.IdBlankType == (byte)BlankType.FormationGRBS);
            foreach (var p in typeof(ISBP_Blank).GetProperties())
            {
                if (((byte?)bl.GetValue(p.Name) ?? (byte)BlankValueType.Empty) != (byte)BlankValueType.Empty)
                {
                    fields += p.Name.Replace("IdBlankValueType_", "Id") + ",";
                    fields2 += p.Name.Replace("IdBlankValueType_", "el.Id") + ",";
                    kbkEq += " and (isnull(" + p.Name.Replace("IdBlankValueType_", "{0}.Id") + ",0) = isnull(" + p.Name.Replace("IdBlankValueType_", "{1}.Id") + ",0)) ";
                }
            }

            string query = string.Format(
                "declare @idTool int " +
                "declare @idBudget int " +
                "declare @idPublicLegalFormation int " +
                "declare @idVersion int " +
                "declare @budgetYear int " +
                "declare @idEntityActivityOfSBP int " +

                "set @idTool = {0} " +
                "set @idBudget = {1} " +
                "set @idPublicLegalFormation = {2} " +
                "set @idVersion = {3} " +
                "set @budgetYear = {4} " +
                "set @idEntityActivityOfSBP = {5} " +

                "declare @ValueTypes table (id tinyint) " +

                (
                    IdSourcesDataTools == (byte)SourcesDataTools.PublicInstitutionEstimate
                    ? "insert into @ValueTypes select {6} union all select {7} "
                    : "insert into @ValueTypes select {8} union all select {9} "
                ) +

                "declare @hierDoc table (idActualDoc int, idLinkDoc int); " +

                "with cte as ( " +
                "   select d.id as idActualDoc, d.id as idLinkDoc, d.idParent " +
                "   from ml.ApprovalBalancingIFDB_ActivityOfSBP as s " +
                "       inner join doc.ActivityOfSBP as d on d.id = s.idActivityOfSBP " +
                "   where s.idApprovalBalancingIFDB = @idTool " +
                "   union all " +
                "   select cte.idActualDoc, d.id as idLinkDoc, d.idParent " +
                "   from cte inner join doc.ActivityOfSBP as d on d.id = cte.idParent) " +

                "insert into @hierDoc select distinct idActualDoc, idLinkDoc from cte " +

                "declare @Tasks table (idDoc int, idStrDoc int, idSBP int, idTask int, idHierarchyPeriod int, idLinkSBP int) " +

                "insert into @Tasks " +
                "select distinct " +
                "   a.idOwner as idDoc, a.id as idStrDoc, a.idSBP, tc.id as idTask, t.idHierarchyPeriod, sbp.id as idLinkSBP " +
                "from ml.ApprovalBalancingIFDB_ActivityOfSBP as d " +
                "   inner join tp.ActivityOfSBP_Activity as a on a.idOwner = d.idActivityOfSBP " +
                "   inner join reg.TaskCollection as tc on " +
                "       tc.idActivity = a.idActivity and isnull(tc.idContingent,0) = isnull(a.idContingent,0) " +
                "       and tc.idPublicLegalFormation = @idPublicLegalFormation " +
                "   inner join reg.TaskVolume as t on " +
                "       t.idTaskCollection = tc.id and t.idSBP = a.idSBP and t.idTerminator is null " +
                "   inner join reg.Program as p on " + 
                "       p.id = t.idProgram and t.idTerminator is null and p.idRegistratorEntity = @idEntityActivityOfSBP " +
                "   inner join @hierDoc as hd on hd.idActualDoc = a.idOwner and hd.idLinkDoc = p.idRegistrator " +
                "   inner join ref.SBP as sbp on " +
                (
                    IdSourcesDataTools == (byte)SourcesDataTools.PublicInstitutionEstimate
                    ? "sbp.idParent = a.idSBP and sbp.idSBPType = 3 "
                    : "sbp.id = a.idSBP "
                ) +
                "where d.idApprovalBalancingIFDB = @idTool " +

                "declare @recs table (idOwner int, idMaster int," + fields.Replace(",", " int,") + " idAuthorityOfExpenseObligation int, idOKATO int, " +
                "AdditionalValue bit, idHierarchyPeriod int, Value numeric(18,2)) " +

                "insert into @recs " +
                "select t.idDoc as idOwner, t.idStrDoc as idMaster," + fields2 + " lva.idAuthorityOfExpenseObligation, lva.idOKATO, " +
                "   isnull(lva.HasAdditionalNeed,0) as AdditionalValue, lva.idHierarchyPeriod, sum(lva.Value) as Value " +
                "from @Tasks as t " +
                "   inner join reg.LimitVolumeAppropriations as lva on " + 
                "       lva.idTaskCollection = t.idTask and lva.idHierarchyPeriod = t.idHierarchyPeriod " +
                "   inner join reg.EstimatedLine as el on " +
                "       el.id = lva.idEstimatedLine and el.idSBP = t.idLinkSBP " +
                "   inner join @ValueTypes as fv on fv.id = lva.idValueType " +
                "where " +
                "   lva.idPublicLegalFormation = @idPublicLegalFormation " +
                "   and lva.idBudget = @idBudget " +
                "   and lva.idVersion = @idVersion " +
                "group by " +
                "   t.idDoc, t.idStrDoc," + fields2 + " lva.idAuthorityOfExpenseObligation, lva.idOKATO, isnull(lva.HasAdditionalNeed,0), lva.idHierarchyPeriod " +
                "having sum(lva.Value) != 0 " +

                "insert into tp.ActivityOfSBP_ActivityResourceMaintenance (" + fields + "idAuthorityOfExpenseObligation, idOKATO, idOwner, idMaster, idBudget, IsDocument) " +
                "select distinct " + fields + "idAuthorityOfExpenseObligation, idOKATO, idOwner, idMaster, @idBudget, 1 from @recs " +

                "insert into tp.ActivityOfSBP_ActivityResourceMaintenance_Value (idOwner, idMaster, idHierarchyPeriod, AdditionalValue, Value) " +
                "select r.idOwner, rm.id, r.idHierarchyPeriod, r.AdditionalValue, r.Value " +
                "from @recs as r inner join tp.ActivityOfSBP_ActivityResourceMaintenance as rm on rm.idMaster = r.idMaster and rm.idBudget = @idBudget " +
                string.Format(kbkEq, "rm", "r") +
                "   and (isnull(rm.idAuthorityOfExpenseObligation,0) = isnull(r.idAuthorityOfExpenseObligation,0)) " +
                "   and (isnull(rm.idOKATO,0) = isnull(r.idOKATO,0)) ",

                Id, IdBudget, IdPublicLegalFormation, IdVersion, Budget.Year, ActivityOfSBP.EntityIdStatic,
                (byte)ValueType.Justified, (byte)ValueType.BalancingIFDB_Estimate,
                (byte)ValueType.JustifiedGRBS, (byte)ValueType.BalancingIFDB_ActivityOfSBP
            );

            context.Database.ExecuteSqlCommand(query);
        }


	    public FileStream output;
        public StreamWriter streamWriter;

        /// <summary>   
        /// Операция «Формирование документов»   
        /// </summary>  
        public void CreateDocs(DataContext context)
        {
            ExecuteControl(e => e.AllToolsInclude(context));
            ExecuteControl(e => e.ExistsBlanks(context));

            UpdateBlanks(context);

            CreateLBA(context);

            FillLBA(context);

            bool isCreateAoSBP = BalancingIFDBs.Any(a => a.IdBalancingIFDBType == (byte)BalancingIFDBType.LimitBudgetAllocationsAndActivityOfSBP);

            if (isCreateAoSBP)
            {
                CreateAoSBP(context);

                FillAoSBP(context);
            }

            foreach (var i in BalancingIFDBs)
            {
                i.ExecuteOperation(e => e.IncludeInSetOf(context));
            }
        }

        /// <summary>   
        /// Операция «Вернуть на черновик»   
        /// </summary>  
        public void BackToDraft(DataContext context)
        {
            context.Database.ExecuteSqlCommand(
                "delete ml.ApprovalBalancingIFDB_LimitBudgetAllocations where idApprovalBalancingIFDB = {0} "+
                "delete ml.ApprovalBalancingIFDB_ActivityOfSBP where idApprovalBalancingIFDB = {0} ", 
                Id);

            foreach (var i in BalancingIFDBs)
            {
                i.ExecuteOperation(e => e.ExcludeFromTheSetOf(context));
            }
        }

        /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {
            DateCommit = DateTime.Now;

            ProcessDocs_LimitBudgetAllocations(context);
        }

        /// <summary>
        /// Операция «Отменить обработку»
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            //context.LimitVolumeAppropriations.RemoveAll(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id);
            DateCommit = null;

            UndoProcessDocs_LimitBudgetAllocations(context);
        }

        /// <summary>   
        /// Операция «Завершить»   
        /// </summary>  
        public void Finish(DataContext context)
        {
            int[] ids = this.BalancingIFDBs.Select(s => s.Id).ToArray();

            List<LimitVolumeAppropriations> lva =
                context.LimitVolumeAppropriations.Where(
                    w => w.IdRegistratorEntity == BalancingIFDB.EntityIdStatic && ids.Contains(w.IdRegistrator))
                       .ToList();

            foreach (var r in lva)
            {
                r.IdRegistratorEntity = EntityId;
                r.IdRegistrator = Id;
                r.Value = -r.Value;
            }

            context.LimitVolumeAppropriations.InsertAsTableValue(lva, context);

            foreach (var r in lva) context.Entry(r).State = EntityState.Unchanged;
        }

        /// <summary>
        /// Операция «Отменить завершение»
        /// </summary>  
        public void UndoFinish(DataContext context)
        {
            context.LimitVolumeAppropriations.RemoveAll(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id);
        }
    }
}