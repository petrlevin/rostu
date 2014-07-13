using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision.Extensions;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class ResultItem
    {
        public static ResultItem GetDifferent(DataRow rowA, DataRow rowB, List<string> changedFields)
        {
            return new ResultItem()
            {
                ItemType = ResultItemType.Different,
                RowA = rowA,
                RowB = rowB,
                ChangedFields = changedFields
            };
        }

        public static ResultItem GetAdded(DataRow rowB)
        {
            return new ResultItem()
                {
                    ItemType = ResultItemType.Added,
                    RowB = rowB
                };
        }

        public static ResultItem GetDeleted(DataRow rowA)
        {
            return new ResultItem()
            {
                ItemType = ResultItemType.Deleted,
                RowA = rowA
            };
        }

        public static ResultItem GetChanged(DataRow rowA, DataRow rowB, List<string> changedFields)
        {
            return new ResultItem()
                {
                    ItemType = ResultItemType.Changed,
                    RowA = rowA,
                    RowB = rowB,
                    ChangedFields = changedFields
                };
        }

        public static ResultItem GetUnchanged(DataRow rowA, DataRow rowB)
        {
            return new ResultItem()
            {
                ItemType = ResultItemType.Unchanged,
                RowA = rowA,
                RowB = rowB                
            };
        }

        private ResultItem()
        {
        }

        public List<string> ChangedFields { get; private set; }

        public ResultItemType ItemType { get; set; }

        public int? IdA { get { return RowA == null ? (int?)null : RowA.GetId(); } }
        public int? IdB { get { return RowB == null ? (int?)null : RowB.GetId(); } }

        public DataRow RowA { get; private set; }
        public DataRow RowB { get; private set; }

        public bool MayBeSame
        {
            get { return ItemType == ResultItemType.Changed || ItemType == ResultItemType.Unchanged; }
        }
    }
}
