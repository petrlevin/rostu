using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using BaseApp;
using Platform.BusinessLogic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using SourcesData = Sbor.DbEnums.SourcesDataTools;


namespace Sbor.Tool
{
    /// <summary>
    /// Балансировка доходов, расходов и ИФДБ
    /// </summary>
    public partial class BalancingIFDB
    {
        public void FillData(DataContext context)
        {
            context.Database.ExecuteSqlCommand("EXEC tool.BalancingIFDB_fillData {0}", Id);
        }

        public static void ApplyRule(DataContext context, int IdRule)
        {
            context.Database.ExecuteSqlCommand("EXEC tool.BalancingIFDB_ApplyRule {0}", IdRule);
        }

        public static List<string> DisableFieldsExpense(DataContext context, int IdExp)
        {
            string[] exists =
                context.BalancingIFDB_EstimatedLine.Where(w => w.IdMaster == IdExp)
                       .Select(
                           s =>
                           new { num = s.HierarchyPeriod.Year - s.Owner.Budget.Year, isAdd = s.IsAdditionalNeed })
                       .Distinct().ToList()
                       .Select(
                           s =>
                           "Change" + (s.isAdd ? "Additional" : "") +
                           (s.num == 0 ? "OFG" : "PFG" + s.num.ToString(CultureInfo.InvariantCulture))).ToArray();

            List<string> all = context.EntityField.Where(w => w.IdEntity == BalancingIFDB_Expense.EntityIdStatic && w.Name.StartsWith("Change")).Select(s => s.Name).ToList();

            return all.Where(w => !exists.Contains(w)).ToList();
        }

        public Dictionary<string, IEnumerable<string>> HideShowFieldsExpense(DataContext context)
        {
            List<string> allFields = context.EntityField.Where(w =>
                w.IdEntity == BalancingIFDB_Expense.EntityIdStatic &&
                w.IdEntityFieldType == (byte)EntityFieldType.Link
                && !(new string[] { "idOwner", "id", "idMaster" }).Contains(w.Name)
            ).Select(s => s.Name).ToList();

            List<string> showFields = new List<string>();
            List<string> hideFields = new List<string>();
            Dictionary<string, IEnumerable<string>> res = new Dictionary<string, IEnumerable<string>>()
                {
                    {"showFields", showFields},
                    {"hideFields", hideFields}
                };

            if (!context.BalancingIFDB_Expense.Any(a => a.IdOwner == Id))
            {
                List<string> selectedFields = context.BalancingIFDB_SetShowKBK.Where(w => w.IdOwner == Id).Select(s => s.EntityField.Name).ToList();
                if (selectedFields.Any())
                {
                    showFields.AddRange(selectedFields);
                }
                else
                {
                    showFields.AddRange(allFields);
                }
            }
            else
            {
                string lst = context.BalancingIFDB_Expense.Where(a => a.IdOwner == Id).GroupBy(g => true).Select(s =>
                    (s.Sum(c => c.IdExpenseObligationType.HasValue ? 1 : 0) > 0 ? ",idExpenseObligationType" : "") +
                    (s.Sum(c => c.IdFinanceSource.HasValue ? 1 : 0) > 0 ? ",idFinanceSource" : "") +
                    (s.Sum(c => c.IdKFO.HasValue ? 1 : 0) > 0 ? ",idKFO" : "") +
                    (s.Sum(c => c.IdKVSR.HasValue ? 1 : 0) > 0 ? ",idKVSR" : "") +
                    (s.Sum(c => c.IdRZPR.HasValue ? 1 : 0) > 0 ? ",idRZPR" : "") +
                    (s.Sum(c => c.IdKCSR.HasValue ? 1 : 0) > 0 ? ",idKCSR" : "") +
                    (s.Sum(c => c.IdKVR.HasValue ? 1 : 0) > 0 ? ",idKVR" : "") +
                    (s.Sum(c => c.IdKOSGU.HasValue ? 1 : 0) > 0 ? ",idKOSGU" : "") +
                    (s.Sum(c => c.IdDFK.HasValue ? 1 : 0) > 0 ? ",idDFK" : "") +
                    (s.Sum(c => c.IdDKR.HasValue ? 1 : 0) > 0 ? ",idDKR" : "") +
                    (s.Sum(c => c.IdDEK.HasValue ? 1 : 0) > 0 ? ",idDEK" : "") +
                    (s.Sum(c => c.IdCodeSubsidy.HasValue ? 1 : 0) > 0 ? ",idCodeSubsidy" : "") +
                    (s.Sum(c => c.IdBranchCode.HasValue ? 1 : 0) > 0 ? ",idBranchCode" : "") +
                    (s.Sum(c => c.IdAuthorityOfExpenseObligation.HasValue ? 1 : 0) > 0 ? ",idAuthorityOfExpenseObligation" : "") +
                    (s.Sum(c => c.IdOKATO.HasValue ? 1 : 0) > 0 ? ",idOKATO" : "")
                ).Single();

                showFields.AddRange(lst.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }

            hideFields.AddRange(allFields.Where(w => !showFields.Contains(w)));

            return res;
        }
    }
}
