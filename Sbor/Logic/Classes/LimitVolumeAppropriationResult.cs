using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Sbor.Interfaces;
using Sbor.Registry;

namespace Sbor.Logic
{
    /// <summary>
    /// Данные из регистра LimitVolumeAppropriations сгруппированные с данными из ТЧ
    /// </summary>
    public class LimitVolumeAppropriationResult : ILineCost, IEquatable<LimitVolumeAppropriationResult>
    {
        /// <summary>
        /// Значение из ТЧ
        /// </summary>
        public decimal? Value { get; set; }
        /// <summary>
        /// План
        /// </summary>
        public decimal? PlanValue { get; set; }
        /// <summary>
        /// Доведено
        /// </summary>
        public decimal? BringValue { get; set; }

        /// <summary>
        /// Обосновано
        /// </summary>
        public decimal? JustifiedValue { get; set; }


        public int Year { get; set; }

        /// <summary>
        /// Получить строку кодов КБК
        /// </summary>
        /// <returns></returns>
        public EstimatedLine GetEstimatedLine(DataContext context)
        {
            var eLine = new EstimatedLine()
                {
                    IdExpenseObligationType = IdExpenseObligationType,
                    IdFinanceSource = IdFinanceSource,
                    IdKFO = IdKFO,
                    IdKVSR = IdKVSR,
                    IdRZPR = IdRZPR,
                    IdKCSR = IdKCSR,
                    IdKVR = IdKVR,
                    IdKOSGU = IdKOSGU,
                    IdDFK = IdDFK,
                    IdDKR = IdDKR,
                    IdDEK = IdDEK,
                    IdCodeSubsidy = IdCodeSubsidy,
                    IdBranchCode = IdBranchCode
                };

            eLine = eLine.GetEstimatedLine(context) ?? eLine.GetEstimatedLineNotExisted(context);

            return eLine;
        }

        /// <summary>
        /// Записи в регистре
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IQueryable<LimitVolumeAppropriations> GetQueryForExistingReg(DataContext context, string alias = "")
        {
            var result = (IQueryable<LimitVolumeAppropriations>)context.LimitVolumeAppropriations;

            if (!String.IsNullOrEmpty(alias))
                alias = alias + ".";
            else
                alias = String.Empty;

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pValue = p.GetValue(this) as int? ?? p.GetValue(this) as byte?;
                if (pValue.HasValue)
                    result = result.Where(String.Format("{2}{0} == {1}", p.Name, pValue.Value, alias));
            }

            return result;
        }

        public string GetWhereQuery(string alias = "")
        {
            var query = new StringBuilder();

            if (!String.IsNullOrEmpty(alias))
                alias = alias + ".";
            else
                alias = String.Empty;

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pValue = p.GetValue(this) as int? ?? p.GetValue(this) as byte?;
                if (pValue.HasValue)
                    query.Append(String.Format(" And {2}{0} = {1} ", p.Name, pValue.Value, alias));
            }

            return query.ToString();
        }

        /// <summary>
        /// Тип РО
        /// </summary>
        public byte? IdExpenseObligationType { get; set; }

        /// <summary>
        /// Источник финансирования
        /// </summary>
        public int? IdFinanceSource { get; set; }

        /// <summary>
        /// КФО
        /// </summary>
        public int? IdKFO { get; set; }

        /// <summary>
        /// КВСР
        /// </summary>
        public int? IdKVSR { get; set; }

        /// <summary>
        /// РЗПР
        /// </summary>
        public int? IdRZPR { get; set; }

        /// <summary>
        /// КЦСР
        /// </summary>
        public int? IdKCSR { get; set; }

        /// <summary>
        /// КВР
        /// </summary>
        public int? IdKVR { get; set; }

        /// <summary>
        /// КОСГУ
        /// </summary>
        public int? IdKOSGU { get; set; }

        /// <summary>
        /// ДФК
        /// </summary>
        public int? IdDFK { get; set; }

        /// <summary>
        /// ДКР
        /// </summary>
        public int? IdDKR { get; set; }

        /// <summary>
        /// ДЭК
        /// </summary>
        public int? IdDEK { get; set; }

        /// <summary>
        /// Код субсидии
        /// </summary>
        public int? IdCodeSubsidy { get; set; }

        /// <summary>
        /// Отраслевой код
        /// </summary>
        public int? IdBranchCode { get; set; }

        public bool Equals(LimitVolumeAppropriationResult other)
        {
            if (this.Year != other.Year)
                return false;

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var thisValue = p.GetValue(this) as int? ?? p.GetValue(this) as byte?;
                var otherValue = p.GetValue(other) as int? ?? p.GetValue(this) as byte?;
                if (thisValue != otherValue)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashResult = Year.GetHashCode();

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pValue = p.GetValue(this) as int? ?? p.GetValue(this) as byte?;

                pValue = pValue ?? 0;

                hashResult ^= pValue.GetHashCode();
            }

            return hashResult;
        }

    }
}
