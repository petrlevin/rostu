using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capricorn.Configuration;
using Platform.BusinessLogic.Common.Exceptions;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class TransactionDeadlocked : ConfigurationSection, ITransactionDeadlocked
    {
        private static ITransactionDeadlocked _instance;

        public static TransactionDeadlockedException CreateException(Exception inner)
        {
            return new TransactionDeadlockedException(_instance.Message,inner);
        }

        public static TransactionDeadlockedException CreateException()
        {
            return new TransactionDeadlockedException(_instance.Message);
        }

        public static Int32 GetAttempsCount()
        {
            return _instance.AttempsCount;
        }

        [ConfigurationProperty("message", IsRequired = true)]
        public MessageBodyElement MessageBody
        {
            get { return (MessageBodyElement)(base["message"]); }
            set { base["message"] = value; }
        }



        public string Message
        {
            get { return MessageBody.Content; }

        }

        [ConfigurationProperty("attempsCount", DefaultValue = "5", IsRequired = false)]
        public Int32 AttempsCount
        {
            get
            {
                return (Int32)this["attempsCount"];
            }
            set
            {
                this["attempsCount"] = value;
            }
        }


        static TransactionDeadlocked()
        {

            _instance = GetInstance();

        }

        private static ITransactionDeadlocked GetInstance()
        {

            try
            {
                var result = 
                    (TransactionDeadlocked) System.Configuration.ConfigurationManager.GetSection(
                        "transactionDeadlocked");
                return result ?? GetDefaultTransactionDeadlocked();


            }
            catch (Exception)
            {

                return GetDefaultTransactionDeadlocked();
            }
        }

        private static ITransactionDeadlocked GetDefaultTransactionDeadlocked()
        {
            return new DefaultTransactionDeadlocked()
                       {
                           AttempsCount = 5,
                           Message = "Выполнить операцию не удалось, попробуйте еще раз. <br/>"
                       };
        }


        class DefaultTransactionDeadlocked : ITransactionDeadlocked
        {
            public string Message { get; set; }
            public Int32 AttempsCount { get; set; }
        }


        public class MessageBodyElement
            : CDataConfigurationElement
        {
            [ConfigurationProperty("content", IsRequired = true, IsKey = true)]
            [CDataConfigurationProperty]
            public string Content
            {
                get { return (string)(base["content"]); }
                set { base["content"] = value; }
            }
        }


    }

    public interface ITransactionDeadlocked
    {
        string Message { get; }
        Int32 AttempsCount { get; }
    }

}
