namespace Platform.Environment.Interfaces
{
    public interface IStorageContainer<TApplicationStorage, TSessionStorage,TRequestStorage> 
        where TRequestStorage:IRequestStorageBase
	{
		TApplicationStorage ApplicationStorage { get; }
		TSessionStorage SessionStorage { get; }
		TRequestStorage RequestStorage { get; }
	}


}
