namespace Platform.PrimaryEntities.Common.Interfaces
{
    public interface IFiller<in TData>
    {
        void Fill(IBaseEntity objectToFill, TData data);
    }
}
