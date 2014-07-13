using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Utils.Extensions;


namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public static class Controls
    {

        /// <summary>
        /// Вспомогательный метод, облегчающий код контролей. 
        /// Если переданный массив содержит хотябы один элемент, то выбрасывается исключение <see cref="ControlResponseException"/>.
        /// </summary>
        /// <param name="wrongItems">Информационный список, который будет выведен в сообщении контроля в указанном с помощью '{0}' месте</param>
        /// <param name="message">Текст сообщения</param>
        public static void Check(List<string> wrongItems, string message)
        {
            if (wrongItems.Any())
            {
                var say = message;
                if (message.Contains("{0}"))
                {
                    say = string.Format(message, wrongItems.ToString("<br>"));
                }
                Throw(say);
            }
        }

        /// <summary>
        /// Контрол срабатывает
        /// </summary>
        /// <param name="message"></param>
        public static void Throw(string message)
        {
            var context = ControlContext.Current;
            if (context!=null)
                context.Throw(message);
            else
                throw new ControlResponseException(message,null,null);
        }



    }
}
