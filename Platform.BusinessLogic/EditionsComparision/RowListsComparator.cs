using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class RowListsComparator
    {
        public static List<ResultItem> Compare(TablepartInfo tpInfo, IEnumerable<DataRow> rowsA, IEnumerable<DataRow> rowsB)
        {
            var instance = new RowListsComparator()
                {
                    TpInfo = tpInfo,
                    RowsA = rowsA,
                    RowsB = rowsB
                };
            return instance.compare();
        }

        private RowListsComparator()
        {

        }
        
        // параметры класса

        private TablepartInfo TpInfo { get; set; }

        private IEnumerable<DataRow> RowsA { get; set; }

        private IEnumerable<DataRow> RowsB { get; set; }

        // Private Fields

        /// <summary>
        /// Оставшиеся (отсутствующие в результате Result) строки
        /// </summary>
        private IEnumerable<DataRow> leftRowsA
        {
            get { return RowsA.Where(row => !Result.Any(res => res.IdA == row.GetId())); }
        }

        /// <summary>
        /// Оставшиеся (отсутствующие в результате Result) строки
        /// </summary>
        private IEnumerable<DataRow> leftRowsB
        {
            get { return RowsB.Where(row => !Result.Any(res => res.IdB == row.GetId())); }
        }

        private List<ResultItem> Result;

        #region Алгоритм сравнения

        private List<ResultItem> compare()
        {
            // если в сравниваемых наборах находится по одной строке, то считаем, что строка изменена (а не удалена и добавлена в случае несовпадения)
            if (RowsA.Count() == 1 && RowsB.Count() == 1)
            {
                ResultItem result = RowsComparator.Compare(TpInfo, RowsA.First(), RowsB.First());

                // при сравнении двух строк (= двух редакций документа) предотвращаем возврат типа Different
                if (result.ItemType != ResultItemType.Unchanged)
                    result.ItemType = ResultItemType.Changed;
                return new List<ResultItem>() { result };
            }

            Result = new List<ResultItem>();
            // равные и измененные строки ТЧ
            getSame();
            // созданные строки ТЧ
            Result.AddRange(leftRowsB.Select(row => ResultItem.GetAdded(row)));
            // удаленные строки ТЧ
            Result.AddRange(leftRowsA.Select(row => ResultItem.GetDeleted(row)));

            return Result;
        }

        /// <summary>
        /// Получаем похожие строки - равные и измененные. Same = Changed || Unchanged
        /// </summary>
        private void getSame()
        {
            foreach (DataRow rowA in RowsA)
            {
                List<ResultItem> results = new List<ResultItem>();
                foreach (DataRow rowB in leftRowsB)
                {
                    ResultItem result = RowsComparator.Compare(TpInfo, rowA, rowB);
                    if (result.MayBeSame)
                        results.Add(result);
                }

                // одинаковые (unchanged) строки ищем для всех ТЧ (даже для тех, у которых нет подчиненных)
                // с той целью, чтобы правильнее определить измененные.
                ResultItem same = results
                    .OrderBy(res => res.ItemType == ResultItemType.Unchanged ? 0 : 1)
                    .FirstOrDefault();

                if (same != null)
                    Result.Add(same);
            }
        }

        #endregion

        /// <summary>
        /// Проверка на наличие в наборе строк, которые по алгоритму проверки считаются одинаковыми.
        /// Например одинаковое значение поля наименования. 
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private Dictionary<int, int> checkForDublicates(IEnumerable<DataRow> rows)
        {
            throw new NotImplementedException();
            return new Dictionary<int, int>();
        }
    }
}
