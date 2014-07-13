using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Common.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Русское представление перечисления
        /// Элементы перечесления должны быть помечены <see cref="EnumCaptionAttribute"/>
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string Caption(this Enum @enum)
        {
            if (@enum != null)
            {
                var attribs = @enum.GetType()
                                   .GetField(@enum.ToString())
                                   .GetCustomAttributes(typeof (EnumCaptionAttribute), false);

                if (attribs.Length > 0)
                    return ((EnumCaptionAttribute) attribs[attribs.Length - 1]).Value;
            }
            return "";
        }
    }
}
