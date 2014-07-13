using Platform.BusinessLogic.EntityTypes;
using Platform.Common.Exceptions;
using Platform.Common.Interfaces;

namespace Platform.BusinessLogic.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ToolStateException : PlatformException , IUserFriendlyException
    {
        private readonly ToolEntity _toolEntity;


        public ToolStateException(string message, ToolEntity toolEntity, string operationDiscription = "Выполнение операции")
            : base(message)
        {
            _toolEntity = toolEntity;
            _operationDescription = operationDiscription;

        }

        public ToolStateException(string message)
            : base(message)
        {


        }


        public ToolEntity ToolEntity
        {
            get { return _toolEntity; }
        }

        private string _operationDescription = "Выполнение операции";

        public string OperationDescription
        {
            get { return _operationDescription; }
        }

        public virtual string ExceptionTypeDescription
        {
            get { return   "Состояние документа"; }
        }
    }
}
