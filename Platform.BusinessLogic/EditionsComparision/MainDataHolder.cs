using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Platform.Common;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.BusinessLogic.EditionsComparision
{
    class MainDataHolder
    {
        #region Public

        public MainDataHolder(ITableInfo tableInfo, int editionA, int editionB)
        {
            connection = IoC.Resolve<SqlConnection>("DbConnection");
            this.tableInfo = tableInfo;
            this.editionA = editionA;
            this.editionB = editionB;
        }

        public ResultItem Compare()
        {
            var result = RowsComparator.Compare(tableInfo, rowA, rowB);
            // при сравнении двух строк (= двух редакций документа) предотвращаем возврат типа Different
            if (result.ItemType != ResultItemType.Unchanged)
                result.ItemType = ResultItemType.Changed;
            return result;
        }

        public void Fill()
        {
            SqlDataAdapter da;
            da = getSqlCmd(editionA);
            da.Fill(tableA);
            da = getSqlCmd(editionB);
            da.Fill(tableB);
        }

        #endregion


        #region Private

        // Параметры класса

        private ITableInfo tableInfo;

        private int editionA;

        private int editionB;

        // Private Fields

        private SqlConnection connection { get; set; }

        private DataTable tableA = new DataTable();

        private DataTable tableB = new DataTable();

        private DataRow rowA
        {
            get { return tableA.Rows[0]; }
        }

        private DataRow rowB
        {
            get { return tableB.Rows[0]; }
        }

        // Methods

        private SqlDataAdapter getSqlCmd(int id)
        {
            SelectQueryBuilder builder = new SelectQueryBuilder(tableInfo.TableEntity);
            builder.Conditions = new FilterConditions()
            {
                Field = "Id",
                Operator = ComparisionOperator.Equal,
                Value = id
            };

            return new SqlDataAdapter(builder.GetSqlCommand(connection));
        }

        #endregion
    }
}
