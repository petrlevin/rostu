namespace Platform.Utils.LazyProperties
{
    public interface ILazyPropertyAnalyzer
    {
        object GetValue();
        bool HasValue();
        bool IsRequired();
    }
}
