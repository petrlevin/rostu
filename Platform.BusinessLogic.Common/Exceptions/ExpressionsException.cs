using System;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение при вычислении выражения для объекта ValueContainer
    /// </summary>
    public class ExpressionsException :PlatformException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inputExpression"></param>
        /// <param name="outputExpression"></param>
        /// <param name="valueContainer"></param>
        public ExpressionsException(string message, string inputExpression, string outputExpression, object valueContainer) : base(message)
        {
            InputExpression = inputExpression;
            OutputExpression = outputExpression;
            ValueContainer = valueContainer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputExpression"></param>
        /// <param name="outputExpression"></param>
        /// <param name="valueContainer"></param>
        public ExpressionsException(string inputExpression, string outputExpression, object valueContainer)
        {
            InputExpression = inputExpression;
            OutputExpression = outputExpression;
            ValueContainer = valueContainer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="inputExpression"></param>
        /// <param name="outputExpression"></param>
        /// <param name="valueContainer"></param>
        public ExpressionsException(string message, Exception innerException, string inputExpression, string outputExpression, object valueContainer)
            : base(message, innerException)
        {
            InputExpression = inputExpression;
            OutputExpression = outputExpression;
            ValueContainer = valueContainer;
        }


        /// <summary>
        /// входное выражение 
        /// например "5 + {Number}"
        /// </summary>
        public string InputExpression { get; set; }

        /// <summary>
        /// выражение после подстановки значений из объекта 
        /// напрмер "5+5"
        /// </summary>
        public string OutputExpression { get; set; }

        /// <summary>
        /// объект содержащий значения для подстановки в выражение
        /// </summary>
        public object ValueContainer  { get; set; }

    }
    
}
