using System;
using System.Text.RegularExpressions;
using Platform.BusinessLogic.Common.Exceptions;

namespace Platform.BusinessLogic.Activity.Values
{
    /// <summary>
    /// 
    /// </summary>
    public class Evaluator
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="valueContainer"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="Microsoft.JScript.JScriptException"></exception>
        /// <exception cref="ValueResolutionException"></exception>
        /// <exception cref="ValueExecutionException"></exception>
        public object Evaluate(string expression, object valueContainer, Options options = Options.Default)
        {
            string parsed = null;
            
            try
            {
                var expr = expression.Trim();
                Match match = Regex.Match(expr);
                if (match.Success)
                    return valueContainer.GetValue(match.Groups["property"].Value, options);
                var ev = new JavascriptEvaluator.Evaluator();
                parsed = _parser.Parse(expr, valueContainer, options);
                return ev.Eval(parsed);

            }
            catch (Exception ex)
            {
                if (parsed == null)
                    throw new ExpressionsException(String.Format("Не возможно вычислить выражение \" {0}\" для объекта \"{1}\"",expression,valueContainer),ex,expression, parsed, valueContainer);
                else
                    throw new ExpressionsException(String.Format("Не возможно вычислить выражение \" {0}\" для объекта \"{1}\". Выражение после подстановки - \" {2}\" ", expression, valueContainer,parsed), ex, expression, parsed, valueContainer);
            }

        }




        private readonly Parser _parser = new Parser();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="valueContainer"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="Microsoft.JScript.JScriptException"></exception>
        /// <exception cref="ValueResolutionException"></exception>
        /// <exception cref="ValueExecutionException"></exception>
        public static object Eval(string expression, object valueContainer, Options options = Options.Default)
        {
            return new Evaluator().Evaluate(expression, valueContainer, options);
        }

        private static readonly Regex Regex = new Regex(@"^\{(?<property>[\w\.]+)\}$", RegexOptions.Compiled | RegexOptions.CultureInvariant |
                                        RegexOptions.IgnoreCase);




    }



}
