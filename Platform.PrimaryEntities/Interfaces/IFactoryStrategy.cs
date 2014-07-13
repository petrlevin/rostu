using System.Data;
using System.Data.SqlClient;

namespace Platform.PrimaryEntities.Interfaces
{
    public interface IFactoryStrategy<TData>
    {
        TResult GetResult<TResult>(ISelect<TData> select, IBaseFactoryElement<TResult> factoryElement)
            where TResult : class;

        ISelect<TData> CreateSelect(object parameter, string parameterName, string metadataName);


        IFactory GetFactory();
    }
}