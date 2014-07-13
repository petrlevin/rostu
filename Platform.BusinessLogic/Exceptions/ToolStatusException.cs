using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Reference;
using Platform.Common.Interfaces;

namespace Platform.BusinessLogic.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ToolStatusException : ToolStateException , IUserFriendlyException
    {

        private readonly DocStatus _docStatus;

        public ToolStatusException(string message ,ToolEntity toolEntity, DocStatus docStatus):base(message, toolEntity)
        {

            _docStatus = docStatus;
        }

        public ToolStatusException(string message, ToolEntity toolEntity) :base(message,toolEntity)
        {
            
        }


        public DocStatus DocStatus
        {
            get { return _docStatus; }
        }


        public override string ExceptionTypeDescription
        {
            get { return "Не правильный статус документа"; }
        }
    }
}
