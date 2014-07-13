using Platform.Common.Exceptions;
using Platform.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
	public class LockEntityException : PlatformException, IUserFriendlyException
    {
        private IBaseEntity _baseEntity;

        public LockEntityException(IBaseEntity baseEntity, string message)
            : base(message)
        {
            _baseEntity = baseEntity;
        }

        public IBaseEntity BaseEntity
        {
            get { return _baseEntity; }
        }

	    #region Implementation of IUserFriendlyException

	    /// <summary>
	    /// Описание совершаемой операции
	    /// </summary>
	    public string OperationDescription { get; private set; }

	    /// <summary>
	    /// Описание типа ошибки
	    /// </summary>
	    public string ExceptionTypeDescription { get; private set; }

	    #endregion
    }
}
