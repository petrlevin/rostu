using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Platform.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Исключение должно выкидываться прикладным кодом
    /// когда контрол не прошел
    /// <example>
    ///   Пример кода:
    /// <code>
    /// if (!CheckState())
    ///     Controls.Throw("Объект прикладной сущности находится в плохом состоянии ");
    /// </code>
    ///  </example>
    /// 
    /// </summary>
    public class ControlResponseException : ControlTargetException, IHandledException
    {
        /// <summary>
        /// 
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UNK { get; set; }

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="control"></param>
        /// <param name="target"></param>
        public ControlResponseException(string message, Exception innerException, MemberInfo control, IBaseEntity target)
            : base( FormatNumbers(message), innerException, control, target)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="control"></param>
        /// <param name="target"></param>
        public ControlResponseException(string message, MemberInfo control, IBaseEntity target)
            : base(FormatNumbers(message), control, target)
        {
        }

        public string ClientHandler
        {
            get { return "Control"; }
        }


        /// <summary>
        /// парсит сформированное сообщение контроля, определяет числовые данные и форматирует их с разделением на разряды
        /// Данные выводимые с разделением групп разрядов должны быть длиннее 4х символов, чтобы исключить вывод года 2014 как 2 014.
        /// </summary>
        private static string FormatNumbers(string InString)
        {
            //string OutString = InString;
            //string[] Split = InString.Split(new Char[] { ' ', '.', ':', ';', '"', '/', '?', '!', '%', '<', '>' });
            //decimal number;
            //foreach (string SplitString in Split)
            //{
            //    var SplitStringTrim = SplitString.Trim(new Char[] {','});

            //    if (decimal.TryParse(SplitStringTrim, out number) && SplitStringTrim.Length > 4)
            //    {
            //       string razr =
            //            ((number == Decimal.Floor(number)) ? 0 : (number - Decimal.Floor(number)).ToString().Length - 2)
            //                .ToString();
            //       OutString = OutString.Replace(SplitStringTrim, number.ToString("N" + razr));
            //    }
            //}


            var OutString = "";
            decimal number;

            string[] PartsString = InString.Split(new Char[] { ' ' });

            foreach (string PartString in PartsString)
            {
                var OutWord = PartString;

                string[] Words = PartString.Split(new Char[] { ' ', '.', ':', ';', '"', '/', '?', '!', '%', '<', '>' });
                foreach (string WordtString in Words)
                {
                    var WordtStringTrim = WordtString.Trim(new Char[] { ',' });

                    if (decimal.TryParse(WordtStringTrim, out number) && WordtStringTrim.Length > 4)
                    {
                        string razr =
                            ((number == Decimal.Floor(number))
                                 ? 0
                                 : (number - Decimal.Floor(number)).ToString().Length - 2)
                                .ToString();
                        OutWord = OutWord.Replace(WordtStringTrim, number.ToString("N" + razr));
                    }
                }
                OutString = OutString + " " + OutWord;
            }

            return OutString;

        }
    }
}
