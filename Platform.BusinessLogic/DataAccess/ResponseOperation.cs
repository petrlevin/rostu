using Platform.Client;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    ///Объект операции для клиентской части
    /// </summary>
	public class ResponseOperation : ExtMenuItem
    {
	    /// <summary>
	    /// Атомарная операция
	    /// </summary>
		public bool IsAtomic;
    }
}