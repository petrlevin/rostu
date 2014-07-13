using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.Unity;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring.Strategies
{
    public class UseCacheWithSqlDependency<TSelect> : UseCache<DataRow>, IDbConnectioned where TSelect : SqlDependencySelect, new()
    {
        public UseCacheWithSqlDependency(IFactoryStrategy<DataRow> innerStrategy) : base(innerStrategy)
        {
        }

        [Dependency("DbConnection")]
        public SqlConnection DbConnection { get; set; }

        protected override void PutToCache<TResult>(SelectBase @select, TResult result)
        {
            if (@select is SqlDependencySelect)
                Cache.Put(((SqlDependencySelect)select).SqlDependencyCommand, result, select.MetadataName, select.ParameterName, select.Parameter);
            else
                throw new InvalidOperationException(
                    String.Format("Для работы кэша с зависимостями (SqlDependency)  объект выборки данных должен быть производным от класса 'Platform.PrimaryEntities.Factoring.Select' . Переданный объект имеет тип '{0}' ", select.GetType().FullName));
        }


        public override ISelect<DataRow> CreateSelect(object parameter, string parameterName, string metadataName)
        {
            return new TSelect()
                       {
                           Cache = Cache,
                           Parameter = parameter,
                           ParameterName = parameterName,
                           MetadataName = metadataName,
                           DbConnection = DbConnection
                       };
        }
    }

    public class UseCacheWithSqlDependency : UseCacheWithSqlDependency<SqlDependencySelect> {
        public UseCacheWithSqlDependency(IFactoryStrategy<DataRow> innerStrategy) : base(innerStrategy)
        {
        }
    }
}
