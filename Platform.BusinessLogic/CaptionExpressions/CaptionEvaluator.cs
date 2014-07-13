using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Values;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Common;

namespace Platform.BusinessLogic.CaptionExpressions
{
    public static class CaptionEvaluator
    {
        private static readonly Regex CalculatedScriptExpressionRegex = new Regex(@"^`.+`$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public static bool IsExpression(string caption)
        {
            return CalculatedScriptExpressionRegex.Match(caption).Success;
        }

        public static string CalculateCaptionExpression(string caption)
        {
            if (IsExpression(caption))
            {
                string result = string.Empty;
                try
                {
                    var evaluator = new Evaluator();
                    var defValues = IoC.Resolve<Object>("DeafaultValues");

                    result = evaluator.Evaluate(caption.Trim('`'), defValues).ToString();
                }
                catch (ExpressionsException ex)
                {
                    //Выведем изначальное значение
                    result = caption;
                }
                return result;
            }
            return caption;
        }
    }
}
