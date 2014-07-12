using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic;
using Platform.Common;
using Platform.Common.Exceptions;

namespace Platform.Web.Services
{
    /// <summary>
    /// Сервис для работы с вычисляемыми на клиенте формулами
    /// </summary>
    public class FormulaService
    {
        /// <summary>
        /// Элементы формулы
        /// </summary>
        public class Indicator
        {
            /// <summary>
            /// Наименование
            /// </summary>
            public string Caption;
            
            /// <summary>
            /// Символ в формуле
            /// </summary>
            public string Symbol;

            /// <summary>
            /// Значение по-умолчанию
            /// </summary>
            public decimal? DefaultValue;
        }

        /// <summary>
        /// Получить параметры формулы
        /// </summary>
        /// <param name="idFormula">Идентификатор формулы</param>
        /// <returns></returns>
        public object GetFormulaIndicators(int idFormula)
        {
            var db = IoC.Resolve<DbContext>().Cast<Sbor.DataContext>();
            var formula = db.CalculationFormula.SingleOrDefault(a => a.Id == idFormula);
            if (formula == null)
                throw new PlatformException(string.Format("Не найдена формула с идентификатором {0}", idFormula));

            return new
                {
                    FormulaText = formula.Description,
                    Indicators = formula.IdIndicatorOfCalculationFormula.Select(a => new Indicator
                        {
                            Caption = a.Caption,
                            Symbol = a.Symbol,
                            DefaultValue = a.DefaultValue
                        }
                    )
                };
        }
    }
}