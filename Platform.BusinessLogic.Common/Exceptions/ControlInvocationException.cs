using System.Linq.Expressions;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение выкидывается когда 
    /// </summary>
    public class ControlInvocationException:PlatformException
    {
        private Expression _expression;

        public ControlInvocationException(Expression expression)
        {
            _expression = expression;
        }

        public Expression Expression
        {
            get { return _expression; }

        }
    }
}
