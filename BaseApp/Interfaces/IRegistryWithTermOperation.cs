using BaseApp.Registry;

namespace BaseApp.Interfaces
{
    /// <summary>
    /// Регистр, хранящий информацию о операции, аннулировавшей его
    /// </summary>
    public interface IRegistryWithTermOperation
    {
        /// <summary>
        /// Идентификатор операции
        /// </summary>
        int? IdTerminateOperation { get; set; }
        
        /// <summary>
        /// Аннулирующая операция
        /// </summary>
        ExecutedOperation TerminateOperation { get; set; }
    }
        
}
