using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Platform.BusinessLogic;
using Platform.Common;
using Sbor.DbEnums;

namespace Sbor.Reports.Report
{
    partial class BudgetExpenseStructure
    {
        private string GetBaseFilter(string prefix = "EL")
        {
            if (BaseFilter.Any())
            {
                var filterParts = BuildBaseFilters(prefix).ToList();

                if (filterParts.Any())
                {
                    var query = " And ( " + String.Join(" OR ", filterParts) + " ) ";
                    return query;
                }
            }

            return String.Empty;
        }

        private IEnumerable<string> BuildBaseFilters(string prefix)
        {
            var result = new List<String>();
            foreach (var filter in BaseFilter)
            {
                var temp = BuildBaseFilter(filter, prefix);

                if (!String.IsNullOrEmpty(temp))
                    result.Add(temp);
            }

            return result;
        }

        private string GetFilterChildIds(string scheme, string name, int idParentField, int[] ids)
        {
            return String.Format(@"
                            Select Distinct FilterIds.id 
	                            From {0}.{1} Item
		                            Cross Apply dbo.GetChildrens(Item.id, {2}, 1) FilterIds
	                            Where Item.Id in ({3})", scheme, name, idParentField, String.Join(",", ids));
        }

        private string BuildBaseFilter(IBudgetExpenseStructureFilter filter, string prefix)
        {
            bool hasRule = false;

            var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

            var sb = new StringBuilder(" ( ");

            #region Тип РО
            if (filter.FilterFieldType_ExpenseObligationType.HasValue)
            {
                var ids = filter.ExpenseObligationTypeIds;

                if (ids.Any())
                {
                    hasRule = true;
                    
                    sb.Append(GetFilterString(filter.FilterFieldType_ExpenseObligationType.Value, 
                                            "ExpenseObligationType",
                                            prefix, 
                                            ids));
                }
            }
            #endregion

            #region Источники финансирования
            if (filter.IdFilterFieldType_FinanceSource.HasValue)
            {
                var ids = filter.FinanceSource.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");

                    hasRule = true;

                    sb.Append(GetFilterString(filter.FilterFieldType_FinanceSource.Value,
                                            "FinanceSource",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region КФО
            if (filter.IdFilterFieldType_KFO.HasValue)
            {
                var ids = filter.KFO.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");

                    hasRule = true;

                    sb.Append(GetFilterString(filter.FilterFieldType_KFO.Value,
                                            "KFO",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region КВСР
            if (filter.IdFilterFieldType_KVSR.HasValue)
            {
                var ids = filter.KVSR.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");

                    hasRule = true;

                    sb.Append(GetFilterString(filter.FilterFieldType_KVSR.Value,
                                            "KVSR",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region РзПР
            if (filter.IdFilterFieldType_RZPR.HasValue)
            {
                var ids = filter.RZPR.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "RZPR", -1543503178, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();

                    sb.Append(GetFilterString(filter.FilterFieldType_RZPR.Value,
                                            "RZPR",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region КЦСР
            if (filter.IdFilterFieldType_KCSR.HasValue)
            {
                var ids = filter.KCSR.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "KCSR", -1543503185, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();

                    sb.Append(GetFilterString(filter.FilterFieldType_KCSR.Value,
                                            "KCSR",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region КВР
            if (filter.IdFilterFieldType_KVR.HasValue)
            {
                var ids = filter.KVR.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "KVR", -1879048004, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();

                    sb.Append(GetFilterString(filter.FilterFieldType_KVR.Value,
                                            "KVR",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region КОСГУ
            if (filter.IdFilterFieldType_KOSGU.HasValue)
            {
                var ids = filter.KOSGU.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "KOSGU", -1543503193, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();

                    sb.Append(GetFilterString(filter.FilterFieldType_KOSGU.Value,
                                            "KOSGU",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region ДФК
            if (filter.IdFilterFieldType_DFK.HasValue)
            {
                var ids = filter.DFK.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "DFK", -1543503216, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();

                    sb.Append(GetFilterString(filter.FilterFieldType_DFK.Value,
                                            "DFK",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region ДКР
            if (filter.IdFilterFieldType_DKR.HasValue)
            {
                var ids = filter.DKR.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "DKR", -1543503200, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();
                    
                    sb.Append(GetFilterString(filter.FilterFieldType_DKR.Value,
                                            "DKR",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region ДЭК
            if (filter.IdFilterFieldType_DEK.HasValue)
            {
                var ids = filter.DEK.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "DEK", -1543503208, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();

                    sb.Append(GetFilterString(filter.FilterFieldType_DEK.Value,
                                            "DEK",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region Коды субсидий
            if (filter.IdFilterFieldType_CodeSubsidy.HasValue)
            {
                var ids = filter.CodeSubsidy.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "CodeSubsidy", -1275068379, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();

                    sb.Append(GetFilterString(filter.FilterFieldType_CodeSubsidy.Value,
                                            "CodeSubsidy",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            #region Отраслевые коды
            if (filter.IdFilterFieldType_BranchCode.HasValue)
            {
                var ids = filter.BranchCode.Select(r => r.Id).ToArray();

                if (ids.Any())
                {
                    if (hasRule)
                        sb.Append(" And ");
                    hasRule = true;

                    var query = GetFilterChildIds("ref", "BranchCode", -1610610643, ids);
                    ids = dc.Database.SqlQuery<int>(query).ToArray();
                    
                    sb.Append(GetFilterString(filter.FilterFieldType_BranchCode.Value,
                                            "BranchCode",
                                            prefix,
                                            ids));
                }
            }
            #endregion

            sb.Append(" ) ");

            return hasRule ? sb.ToString() : String.Empty;
        }

        private string GetFilterString(FilterFieldType type, string columnName, string prefix, int[] ids)
        {
            return String.Format(" {0}.id{1} {2} In ({3})", 
                prefix, 
                columnName, 
                (type == FilterFieldType.Except) ? "Not" : String.Empty, 
                string.Join(", ", ids));
        }

    }
}
