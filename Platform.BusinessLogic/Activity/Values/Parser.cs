using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Platform.Caching;
using Platform.Caching.Common;

namespace Platform.BusinessLogic.Activity.Values
{
    /// <summary>
    /// выполняет подстановку в форматную строку значениями из источника (source)
    /// например '5 + {Some} - {Other} ' => '5 + <source.Some/> - <source.Other/> '
    /// если source.Some = 67 и source.Other() возвращает 100 на выходе будет : '5+67-100' 
    /// </summary>
    public  class Parser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="source"></param>
        /// <param name="options"></param>

        /// <returns></returns>
        public string Parse (string expression, object source, Options options = Options.Default)
        {
            var values = new List<Object>();
            
            var rewrittenFormat = Regex.Replace(expression,
                                                
                                                m =>
                                                {
                                                    Group startGroup = m.Groups["start"];
                                                    Group propertyGroup = m.Groups["property"];
                                                    Group formatGroup = m.Groups["format"];
                                                    Group endGroup = m.Groups["end"];
                                                    object value;
                                                    if (source is IDictionary<string, object>)
                                                    {
                                                        ((IDictionary<string, object>)source).TryGetValue(propertyGroup.Value, out value);
                                                    }

                                                    
                                                    else
                                                    {

                                                        value = new Getter().Get(source,
                                                                                     propertyGroup.Value, options);
                                                        
                                                    }
                                                    values.Add(value);

                                                    int openings = startGroup.Captures.Count;
                                                    int closings = endGroup.Captures.Count;

                                                    return openings > closings || openings % 2 == 0
                                                               ? m.Value
                                                               : new string('{', openings) + (values.Count - 1) +
                                                                 formatGroup.Value
                                                                 + new string('}', closings);
                                                });

            return string.Format( rewrittenFormat, values.ToArray());
        }

        private static readonly Regex Regex = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+" ,RegexOptions.Compiled | RegexOptions.CultureInvariant |
                                                RegexOptions.IgnoreCase);



        
    }
}
