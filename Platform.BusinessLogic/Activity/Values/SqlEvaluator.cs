using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Values
{
    public class SqlEvaluator
    {
        private static readonly Regex Regex = new Regex(@"@(?<param>[\w\.\[\]]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant |
                                RegexOptions.IgnoreCase);


        public object Evaluate(string defaultValue, Dictionary<string, object> defaultValues, SqlCommand cmd)
        {
            if (Regex.Match(defaultValue).Success)
            {
                cmd.CommandText = Regex.Replace(defaultValue,

                                               m =>
                                               {
                                                   var param = m.Groups["param"].Value.ToLowerInvariant();

                                                   object value = null;
                                                   if (defaultValues.ContainsKey(param))
                                                       value = defaultValues[param];

                                                   value = value ?? string.Empty;

                                                   int intValue;
                                                   if (Int32.TryParse(value.ToString(), out intValue))
                                                       return intValue.ToString();

                                                   return "'" + value.ToString() + "'";
                                               });
            }
            else
                return null;
            
            return cmd.ExecuteScalar();
        }
            
    }
}
