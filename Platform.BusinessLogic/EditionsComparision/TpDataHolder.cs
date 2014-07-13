using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision.Extensions;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class TpDataHolder
    {
        public TpDataHolder(TablepartInfo tpInfo)
        {
            TpInfo = tpInfo;
            connection = IoC.Resolve<SqlConnection>("DbConnection");
        }

        #region Public Properties

        public TablepartInfo TpInfo { get; protected set; }

        public int OwnerItemIdA { get; set; }

        public int OwnerItemIdB { get; set; }

        public List<ResultItem> Result { get; private set; }

        #endregion


        #region Public Methods

        /// <summary>
        /// Загрузить данные из табличных частей для обеих сравниваемых редакций
        /// </summary>
        public void Fill()
        {
            SqlDataAdapter da;
            da = getSqlCmdForTablepart(OwnerItemIdA);
            da.Fill(tableA);
            da = getSqlCmdForTablepart(OwnerItemIdB);
            da.Fill(tableB);
        }

        public void Compare(TpDataHolder parent)
        {
            Result = new List<ResultItem>();
            if (parent == null)
            {
                // ТЧ TpInfo не является подчиненной
                Result = compare(tableA.Select(), tableB.Select());
            }
            else
            {
                // Табличная часть TpInfo подчинена табличной части parent
                foreach (ResultItem parentResultItem in parent.Result/*.Where(item => item.MayBeSame)*/)
                {
                    DataRow[] rowsA = parentResultItem.ItemType == ResultItemType.Added 
                        ? new DataRow[0]
                        : filterRowsByMaster(tableA, (int)parentResultItem.IdA);
                    DataRow[] rowsB = parentResultItem.ItemType == ResultItemType.Deleted 
                        ? new DataRow[0]
                        : filterRowsByMaster(tableB, (int)parentResultItem.IdB);

                    var r = compare(rowsA, rowsB);
                    Result.AddRange(r);
                }
            }
        }

        // ToDo{SBORIII-1525} избавиться от копипаста внутри метода
        public List<ResultItem> ResultByMaster(ResultItem master)
        {
            if (master == null)
                return Result;

            List<ResultItem> result = new List<ResultItem>();
            List<int> childIds;
            
            if (master.ItemType != ResultItemType.Deleted)
            {
                childIds = filterRowsByMaster(tableB, (int)master.IdB).Select(row => row.GetId()).ToList();
                var r = Result.Where(res => res.IdB.HasValue && childIds.Contains((int)res.IdB));
                result.AddRange(r);
            }

            if (master.ItemType != ResultItemType.Added)
            {
                childIds = filterRowsByMaster(tableA, (int)master.IdA).Select(row => row.GetId()).ToList();
                var r = Result.Where(res => res.IdA.HasValue && childIds.Contains((int)res.IdA));
                result.AddRange(r);
            }

            return result;
        }

        #endregion


        #region Protected Members

        protected DataTable tableA = new DataTable();

        protected DataTable tableB = new DataTable();

        #endregion


        #region Private Members

        private SqlConnection connection { get; set; }

        private SqlDataAdapter getSqlCmdForTablepart(int ownerItemId)
        {
            SelectQueryBuilder builder = new SelectQueryBuilder(TpInfo.TableEntity);
            builder.Conditions = new FilterConditions()
                {
                    Field = TpInfo.OwnerFieldName,
                    Operator = ComparisionOperator.Equal,
                    Value = ownerItemId
                };

            return new SqlDataAdapter(builder.GetSqlCommand(connection));
        }

        private List<ResultItem> compare(DataRow[] rowsA, DataRow[] rowsB)
        {
            return RowListsComparator.Compare(TpInfo, rowsA, rowsB);
        }

        private DataRow[] filterRowsByMaster(DataTable table, int idMasterValue)
        {
            return table.Select(createFilterCondition(idMasterValue));
        }

        private string createFilterCondition(int idMasterValue)
        {
            return string.Format("{0} = {1}", TpInfo.MasterFieldName, idMasterValue);
        }

        #endregion
    }
}
