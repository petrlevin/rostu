namespace Platform.Dal.Interfaces
{
    /// <summary>
    /// Декоратор, реализующий IObservable
    /// </summary>
    public interface IObservableDecorator
    {
        /// <summary>
        /// Событие после применения декоратора
        /// </summary>
        event OnDecoratedHandler Decorated;
    }
}