using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision.Extensions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.PrimaryEntities.Common.Extensions;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class RowsComparator
    {
        public static ResultItem Compare(ITableInfo tpInfo, DataRow rowA, DataRow rowB)
        {
            var instance = new RowsComparator(tpInfo, rowA, rowB);
            return instance.compare();
        }

        private RowsComparator(ITableInfo tpInfo, DataRow rowA, DataRow rowB)
        {
            TpInfo = tpInfo;
            RowA = rowA;
            RowB = rowB;
        }

        // Параметры класса

        private ITableInfo TpInfo;
        private DataRow RowA;
        private DataRow RowB;

        // Private Fields

        private IEnumerable<IEntityField> Fields
        {
            get { return TpInfo.Fields; }
        }

        private ResultItem compare()
        {
            List<string> equalFields = getEqualFieldNames();
            List<string> changedFields = Fields.Select(ef => ef.Name).Where(name => !equalFields.Contains(name)).ToList();

            if (equalFields.Count >= Fields.Count()) // все поля равны (кроме поля id, idOwner, idMaster)
            {
                return ResultItem.GetUnchanged(RowA, RowB);
            }
            else if (TpInfo.HasCaptionField)
            {
                if (equalFields.Contains(TpInfo.CaptionFieldName))          // равны значения поля наименования
                    return ResultItem.GetChanged(RowA, RowB, changedFields);
                return ResultItem.GetDifferent(RowA, RowB, changedFields);
            }
            else if (
                Fields.Where(ef => ef.IsLinkField()).All(ef => equalFields.Contains(ef.Name)) // равны все ссылочные поля
                )
            {
                return ResultItem.GetChanged(RowA, RowB, changedFields);
            }

            return ResultItem.GetDifferent(RowA, RowB, changedFields);
        }

        private List<string> getEqualFieldNames()
        {
            var result = new List<string>();

            foreach (string fieldName in Fields.Select(ef => ef.Name))
            {
                if (RowA[fieldName].Equals(RowB[fieldName]))
                    result.Add(fieldName);
            }
            return result;
        }
    }
}
