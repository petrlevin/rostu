using BaseApp.Registry;

namespace BaseApp.Interfaces
{
    /// <summary>
    /// Регистр, хранящий информацию о операции которой он был записан
    /// </summary>
    public interface IRegistryWithOperation
    {
        /// <summary>
        /// Выполненная операция
        /// </summary>
        ExecutedOperation ExecutedOperation { get; set; }
    }
        
}
