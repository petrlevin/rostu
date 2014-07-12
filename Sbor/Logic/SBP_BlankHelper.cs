using System.Collections.Generic;
using System.Linq;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Tablepart;

namespace Sbor.Logic
{
    /// <summary>
    /// Методы расширения для бланков СБП
    /// </summary>
    public static class SBP_BlankHelper
    {
        private const string PrefixSpbBlankPropertie = "IdBlankValueType_";

        /// <summary>
        /// Проверка заполнения строк в соответствии с бланком доведения
        /// </summary>
        /// <param name="blank"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool CheckByBlank(this ISBP_Blank blank, ILineCost line)
        {
            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pName = p.Name.Substring(2);
                var pValue = p.GetValue(line) as int? ?? p.GetValue(line) as byte?;

                var blankPropertie = typeof(SBP_Blank).GetProperty(PrefixSpbBlankPropertie + pName);
                var blankValue = (byte?)blankPropertie.GetValue(blank);

                if ((!blankValue.HasValue || blankValue == (byte)BlankValueType.Empty) && pValue.HasValue)
                    return false;

                if ((blankValue == (byte)BlankValueType.Mandatory) && !pValue.HasValue)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Приведение строки расходов к бланку
        /// </summary>
        /// <param name="blank">бланк</param>
        /// <param name="line">строка</param>
        /// <param name="isrequired">если только обязательные (иначе еще и не обязательные)</param>
        /// <returns></returns>
        public static Dictionary<string, int?> GetLineReductedToBlank(this ISBP_Blank blank, ILineCost line, bool isrequired)
        {
            return blank.GetLineReductedToBlank(line, isrequired, new Dictionary<string, int?>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blank"></param>
        /// <param name="line"></param>
        /// <param name="isrequired"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Dictionary<string, int?> GetLineReductedToBlank(this ISBP_Blank blank, ILineCost line, bool isrequired, Dictionary<string, int?> values)
        {
            var bvtyp = new List<byte?> { (byte)BlankValueType.Mandatory };
            if (!isrequired)
                bvtyp.Add((byte)BlankValueType.Optional);

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pName = p.Name.Substring(2);
                var pValue = p.GetValue(line) as int? ?? p.GetValue(line) as byte?;

                var blankPropertie = typeof(ISBP_Blank).GetProperty(PrefixSpbBlankPropertie + pName);
                var blankValue = (byte?)blankPropertie.GetValue(blank);

                var value = bvtyp.Contains(blankValue) ? pValue : null;
                if (values.ContainsKey(p.Name) && values[p.Name].HasValue)
                    values[p.Name] = value;
                else
                    values.Add(p.Name, value);
            }

            return values;
        }

        /// <summary>
        /// Приведение строки расходов к общим полям бланков
        /// </summary>
        /// <param name="blanks">бланки</param>
        /// <param name="line">строка</param>
        /// <param name="isrequired">если только обязательные (иначе еще и не обязательные)</param>
        /// <returns></returns>
        public static Dictionary<string, int?> GetLineReductedToBlanks(IEnumerable<ISBP_Blank> blanks, ILineCost line, bool isrequired)
        {
            var result = new Dictionary<string, int?>();

            foreach (var blank in blanks.ToList() )
            {
                result = blank.GetLineReductedToBlank(line, isrequired, result);
            }

            return result;
        }

        /// <summary>
        /// Проверка идентичности бланков
        /// </summary>
        public static bool IsEqualBlank(ISBP_Blank blank_a, ISBP_Blank blank_b)
        {

            return
                (blank_a.IdBlankValueType_BranchCode ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_BranchCode ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_CodeSubsidy ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_CodeSubsidy ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_DEK ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_DEK ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_DFK ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_DFK ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_DKR ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_DKR ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_ExpenseObligationType ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_ExpenseObligationType ?? (byte) BlankValueType.Empty) &&
                blank_a.IdBlankValueType_FinanceSource == blank_b.IdBlankValueType_FinanceSource &&
                (blank_a.IdBlankValueType_KCSR ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_KCSR ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_KFO ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_KFO ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_KOSGU ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_KOSGU ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_KVR ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_KVR ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_KVSR ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_KVSR ?? (byte) BlankValueType.Empty) &&
                (blank_a.IdBlankValueType_RZPR ?? (byte) BlankValueType.Empty) ==
                (blank_b.IdBlankValueType_RZPR ?? (byte) BlankValueType.Empty);
        }

        /// <summary>
        /// Получение бланка "пересечения" списка бланков
        /// </summary>
        /// <param name="blanks">список бланков</param>
        /// <returns></returns>
        public static SBP_Blank GetReductedBlank(List<ISBP_Blank> blanks)
        {
            if (!blanks.Any()) return null;

            var blankR = new SBP_Blank();

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pName = p.Name.Substring(2);

                var blankPropertie = typeof(ISBP_Blank).GetProperty(PrefixSpbBlankPropertie + pName);

                blankPropertie.SetValue(blankR, (byte)BlankValueType.Mandatory); // изначально все считаем обязательными

                foreach (var blank in blanks)
                {
                    var blankValue = (byte?)blankPropertie.GetValue(blank);
                    var blankRValue = (byte?)blankPropertie.GetValue(blankR);

                    if (blankRValue == (byte)BlankValueType.Mandatory ||
                        (blankRValue == (byte)BlankValueType.Optional
                         && blankValue != (byte)BlankValueType.Mandatory && blankValue != (byte)BlankValueType.Optional))
                    {
                        blankPropertie.SetValue(blankR, blankValue);
                    }
                }
            }

            return blankR;
        }

        /// <summary>
        /// Получить список "запрещенных бланком" и обязательных КБК для сметной строки расхода
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public static Dictionary<string, IEnumerable<string>> GetBlankCostProperties(this ISBP_Blank blank)
        {
            var hidden = new List<string>();
            var required = new List<string>();
            if (blank != null)
                foreach (var p in typeof(ILineCost).GetProperties())
                {
                    var pName = p.Name.Substring(2);

                    var blankPropertie = typeof(ISBP_Blank).GetProperty(PrefixSpbBlankPropertie + pName);
                    var blankValue = (byte?)blankPropertie.GetValue(blank);

                    if (!blankValue.HasValue || blankValue == (byte) BlankValueType.Empty)
                        hidden.Add(p.Name.ToLower());
                    else
                        if (blankValue == (byte) BlankValueType.Mandatory)
                            required.Add(p.Name.ToLower());
                }

            return new Dictionary<string, IEnumerable<string>>()
                {
                    {"hidden", hidden},
                    {"required", required}
                };
        }

        /// <summary>
        /// Получить список "запрещенных бланком" КБК для сметной строки расхода
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetBlankCostHiddenProperties(this ISBP_Blank blank)
        {
            var res = new List<string>();
            if (blank != null)
                foreach (var p in typeof(ILineCost).GetProperties())
                {
                    var pName = p.Name.Substring(2);

                    var blankPropertie = typeof(ISBP_Blank).GetProperty(PrefixSpbBlankPropertie + pName);
                    var blankValue = (byte?)blankPropertie.GetValue(blank);

                    if (!blankValue.HasValue || blankValue == (byte)BlankValueType.Empty)
                        res.Add(p.Name.ToLower());
                }

            return res;
        }

        /// <summary>
        /// Получить список обязательных КБК для сметной строки расхода
        /// </summary>
        /// <param name="blank"></param>
        /// <param name="isLowerCase"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetBlankCostMandatoryProperties(this ISBP_Blank blank, bool isLowerCase = false)
        {
            return blank.GetBlankCostMandatoryProperties(null, isLowerCase);
        }

        /// <summary>
        /// Получить список обязательных КБК для сметной строки расхода
        /// </summary>
        /// <param name="blank"></param>
        /// <param name="values"></param>
        /// <param name="isLowerCase"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetBlankCostMandatoryProperties(this ISBP_Blank blank, IEnumerable<string> values, bool isLowerCase = false)
        {
            var result = new List<string>();

            if (blank != null)
                foreach (var p in typeof (ILineCost).GetProperties())
                {
                    var pName = p.Name.Substring(2);

                    var blankPropertie = typeof (ISBP_Blank).GetProperty(PrefixSpbBlankPropertie + pName);
                    var blankValue = (byte?) blankPropertie.GetValue(blank);

                    var name = isLowerCase ? p.Name.ToLower() : p.Name;

                    if (blankValue.HasValue && blankValue == (byte) BlankValueType.Mandatory && (values == null || values.Contains(name) ) )
                        result.Add(name);
                }
            else
            {
                if (values != null)
                    return values;
            }

            return result;
        }

        /// <summary>
        /// Получить список обязательных КБК для сметной строки расхода (для нескольких бланков)
        /// </summary>
        /// <param name="blanks"></param>
        /// <param name="isLowerCase"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetBlanksCostMandatoryProperties(IEnumerable<ISBP_Blank> blanks, bool isLowerCase = false)
        {
            List<string> result = null;

            foreach (var blank in blanks)
                result = blank.GetBlankCostMandatoryProperties(result, isLowerCase).ToList();
            
            return result ?? new List<string>();
        }


        /// <summary>
        /// Получить список "запрещенных бланком" КБК для сметной строки расхода
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<string>> GetBlankCostHiddenAndMandatoryProperties(this ISBP_Blank blank)
        {
            return new List<IEnumerable<string>>() { GetBlankCostHiddenProperties(blank), GetBlankCostMandatoryProperties(blank, true) };
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public static BlankProperties GetBlankProperties(this ISBP_Blank blank)
        {
            var result = new BlankProperties();
            
            if (blank != null)
                foreach (var p in typeof(ILineCost).GetProperties())
                {
                    var pName = p.Name.Substring(2);

                    var blankPropertie = typeof(ISBP_Blank).GetProperty(PrefixSpbBlankPropertie + pName);
                    var blankValue = (byte?)blankPropertie.GetValue(blank);

                    switch (blankValue??(byte)BlankValueType.Empty)
                    {
                        case (byte)BlankValueType.Empty:
                            result.Denied.Add(p.Name);
                            break;
                        case (byte)BlankValueType.Mandatory:
                            result.Mandatory.Add(p.Name);
                            break;
                        case (byte)BlankValueType.Optional:
                            result.Optional.Add(p.Name);
                            break;

                    }
                }

            return result;
        }

        /// <summary>
        /// Получаем список доступных версионных КБК для бланка
        /// </summary>
        /// <param name="blank"></param>
        /// <returns></returns>
        public static BlankVersioningProperties GetVersioningKbk(this ISBP_Blank blank)
        {
            return GetVersioningKbk(blank, new string[]{} );
        }

        /// <summary>
        /// Получаем список доступных версионных КБК для бланка
        /// </summary>
        /// <param name="blank"></param>
        /// <param name="nonVersioned">Игнорируемые поля</param>
        /// <returns></returns>
        public static BlankVersioningProperties GetVersioningKbk(this ISBP_Blank blank, string[] nonVersioned)
        {
            var result = new BlankVersioningProperties();

            var blankProperties = blank.GetBlankProperties();

            foreach (var p in typeof(ILineCostWithRelations).GetProperties())
            {
                var pName = "Id" + p.Name;
                if ( nonVersioned.Contains(pName) || p.PropertyType.GetInterface(typeof(IVersioning).Name) == null)
                {
                    if (blankProperties.Mandatory.Contains(pName))
                        result.MandatoryNonVersioned.Add(p.Name);
                    else if (blankProperties.Optional.Contains(pName))
                        result.OptionalNonVersioned.Add(p.Name);
                }
                else
                {
                    if (blankProperties.Mandatory.Contains(pName))
                        result.MandatoryVersioning.Add(p.Name);
                    else if (blankProperties.Optional.Contains(pName))
                        result.OptionalVersioning.Add(p.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// проверка типа кода бланка на "Пусто"
        /// </summary>
        public static bool IsBvtEmpty(byte? bvt)
        {
            return ((bvt ?? (byte)BlankValueType.Empty) == (byte)BlankValueType.Empty);
        }

        /// <summary>
        /// Получение последнего обязательного кода КБК у бланка
        /// </summary>
        /// <param name="blank">бланк</param>
        /// <returns></returns>
        public static string GetLastObligatoryBlank(this SBP_Blank blank)
        {
            var blankProperties = blank.GetBlankProperties();

            var LastObligatoryBlank = "";

            if (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory) LastObligatoryBlank = "FinanceSource";
            if (blank.BlankValueType_KFO == BlankValueType.Mandatory) LastObligatoryBlank = "KFO";
            if (blank.BlankValueType_KVSR == BlankValueType.Mandatory) LastObligatoryBlank = "KVSR";
            if (blank.BlankValueType_RZPR == BlankValueType.Mandatory) LastObligatoryBlank = "RZPR";
            if (blank.BlankValueType_KCSR == BlankValueType.Mandatory) LastObligatoryBlank = "KCSR";
            if (blank.BlankValueType_KVR == BlankValueType.Mandatory) LastObligatoryBlank = "KVR";
            if (blank.BlankValueType_KOSGU == BlankValueType.Mandatory) LastObligatoryBlank = "KOSGU";
            if (blank.BlankValueType_DFK == BlankValueType.Mandatory) LastObligatoryBlank = "DFK";
            if (blank.BlankValueType_DKR == BlankValueType.Mandatory) LastObligatoryBlank = "DKR";
            if (blank.BlankValueType_DEK == BlankValueType.Mandatory) LastObligatoryBlank = "DEK";
            if (blank.BlankValueType_BranchCode == BlankValueType.Mandatory) LastObligatoryBlank = "BranchCode";
            if (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory) LastObligatoryBlank = "CodeSubsidy";

            return LastObligatoryBlank;
        }

    }
}
