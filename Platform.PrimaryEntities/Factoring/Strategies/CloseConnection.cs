using System;
using System.Data;
using System.Data.SqlClient;
using Platform.PrimaryEntities.Interfaces;
using Platform.Utils.DecoratorPattern;

namespace Platform.PrimaryEntities.Factoring.Strategies
{
    /// <summary>
    /// Стратегия декоратор
    /// закрывает соедиение после получения данных внутреннией стратегией (декорируемой) 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class CloseConnection<TData> : BaseDecorator<IFactoryStrategy<TData>> , IFactoryStrategy<TData>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inner">декорируемая стратегия</param>
        /// <exception cref="ArgumentException"></exception>
        public  CloseConnection(IFactoryStrategy<TData> inner):base(inner)
        {
            if (!(inner is IDbConnectioned))
                throw new ArgumentException(String.Format("В конструктор передана стратегия типа '{0}'  который не реализует интерфес 'IDbConnectioned'",inner));
        }


        public TResult GetResult<TResult>(ISelect<TData> @select, IBaseFactoryElement<TResult> factoryElement) where TResult : class
        {
              TResult result = Inner.GetResult<TResult>(select, factoryElement);

             ((IDbConnectioned)Inner).DbConnection.Close();
            return result;
        }

        public ISelect<TData> CreateSelect(object parameter, string parameterName, string metadataName)
        {
            return Inner.CreateSelect(parameter, parameterName, metadataName);
        }

        public IFactory GetFactory()
        {
            return Inner.GetFactory();
        }
    }

    public class CloseConnection : CloseConnection<DataRow>
    {
        public CloseConnection(SqlConnection dbConnection)
            : base(new Default(dbConnection))
        {

        }

        public CloseConnection(IFactoryStrategy<DataRow> inner)
            : base(inner)
        {

        }
    }

}
